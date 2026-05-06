using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using PuntoVenta.Application.DTOs;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoVenta.Api.Controllers
{
    /// <summary>
    /// Controlador para gestionar clientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailValidationService _emailValidationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ClientesController> _logger;
        private readonly IMediator _mediator;

        public ClientesController(IUnitOfWork unitOfWork, IEmailValidationService emailValidationService, IEmailService emailService, ILogger<ClientesController> logger, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _emailValidationService = emailValidationService;
            _emailService = emailService;
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene todos los clientes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ClienteResponseDto>>> GetClientes()
        {
            try
            {
                var clientes = await _unitOfWork.Clientes.GetAllAsync();
                var result = clientes
                    .Select(c => new ClienteResponseDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Documento = c.Documento,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Activo = c.Activo,
                        FechaCreacion = c.FechaCreacion,
                        FechaEliminacion = c.FechaEliminacion
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Busca clientes por término (nombre, documento, teléfono o email)
        /// Optimizado para autocompletado con miles de registros
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<ClienteResponseDto>>> SearchClientes(
            [FromQuery] string term, 
            [FromQuery] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Ok(new List<ClienteResponseDto>());
                }

                // Limitar el número de resultados para performance
                if (limit > 50) limit = 50;

                var clientes = await _unitOfWork.Clientes.GetAllAsync();
                
                var termLower = term.ToLower().Trim();
                var result = clientes
                    .Where(c => c.Activo && (
                        c.Nombre.ToLower().Contains(termLower) ||
                        c.Apellido.ToLower().Contains(termLower) ||
                        c.Documento.ToLower().Contains(termLower) ||
                        (c.Telefono != null && c.Telefono.ToLower().Contains(termLower)) ||
                        (c.Email != null && c.Email.ToLower().Contains(termLower))
                    ))
                    .Take(limit)
                    .Select(c => new ClienteResponseDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Documento = c.Documento,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Activo = c.Activo,
                        FechaCreacion = c.FechaCreacion,
                        FechaEliminacion = c.FechaEliminacion
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un cliente específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteResponseDto>> GetClienteById(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new { message = $"Cliente con ID {id} no encontrado" });
                }

                var result = new ClienteResponseDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Documento = cliente.Documento,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Activo = cliente.Activo,
                    FechaCreacion = cliente.FechaCreacion,
                    FechaEliminacion = cliente.FechaEliminacion
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo cliente
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<ActionResult<ClienteResponseDto>> CreateCliente([FromBody] CreateClienteDto createClienteDto)
        {
            try
            {
                // Validar que el documento sea único
                if (!string.IsNullOrWhiteSpace(createClienteDto.Documento))
                {
                    var clienteExistente = await _unitOfWork.Clientes
                        .GetByDocumentoAsync(createClienteDto.Documento.Trim());
                    
                    if (clienteExistente != null)
                    {
                        return BadRequest(new { message = $"Ya existe un cliente registrado con el documento: {createClienteDto.Documento}" });
                    }
                }

                // Validar que el correo sea único (si se proporciona)
                if (!string.IsNullOrWhiteSpace(createClienteDto.Email))
                {
                    bool isEmailUnique = await _emailValidationService.IsEmailUniqueAsync(createClienteDto.Email);
                    if (!isEmailUnique)
                    {
                        string? owner = await _emailValidationService.GetEmailOwnerAsync(createClienteDto.Email);
                        return BadRequest(new { message = $"El correo ya está registrado por: {owner}" });
                    }
                }

                var cliente = new PuntoVenta.Domain.Entities.Cliente
                {
                    Nombre = createClienteDto.Nombre ?? string.Empty,
                    Apellido = createClienteDto.Apellido ?? string.Empty,
                    Documento = createClienteDto.Documento ?? string.Empty,
                    Email = createClienteDto.Email ?? string.Empty,
                    Telefono = createClienteDto.Telefono ?? string.Empty,
                    Direccion = createClienteDto.Direccion ?? string.Empty,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Clientes.AddAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                    var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                    var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                    var usuarioNombre = $"{nombre} {apellido}".Trim();

                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = usuarioId,
                        NombreUsuario = usuarioNombre,
                        TipoAccion = "Crear",
                        Modulo = "Clientes",
                        Descripcion = $"Nuevo cliente '{cliente.Nombre} {cliente.Apellido}' creado",
                        RegistroAfectadoId = cliente.Id,
                        RegistroAfectadoDescripcion = $"Cliente: {cliente.Nombre} {cliente.Apellido} (Documento: {cliente.Documento})"
                    });
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning($"⚠️ No se pudo registrar auditoría: {auditEx.Message}");
                }

                var result = new ClienteResponseDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Documento = cliente.Documento,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Activo = cliente.Activo,
                    FechaCreacion = cliente.FechaCreacion,
                    FechaEliminacion = cliente.FechaEliminacion
                };

                // Enviar email de bienvenida si el cliente tiene email
                // COMENTADO: Solo queremos correos al crear/eliminar facturas
                //if (!string.IsNullOrWhiteSpace(cliente.Email))
                //{
                //    try
                //    {
                //        string clienteCompleto = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                //        await _emailService.SendClienteCreatedEmailAsync(cliente.Email, clienteCompleto, cliente.Documento);
                //        _logger.LogInformation($"✅ Email de bienvenida enviado a {cliente.Email}");
                //    }
                //    catch (Exception emailEx)
                //    {
                //        _logger.LogWarning($"⚠️ No se pudo enviar email de bienvenida a {cliente.Email}: {emailEx.Message}");
                //    }
                //}

                return CreatedAtAction(nameof(GetClienteById), new { id = cliente.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cliente");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<ActionResult<ClienteResponseDto>> UpdateCliente(int id, [FromBody] UpdateClienteDto updateClienteDto)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new { message = $"Cliente con ID {id} no encontrado" });
                }

                // Almacenar los valores originales para detectar cambios
                var clienteOriginal = new 
                { 
                    cliente.Nombre, 
                    cliente.Apellido, 
                    cliente.Email, 
                    cliente.Telefono, 
                    cliente.Direccion, 
                    cliente.Activo, 
                    cliente.FechaEliminacion
                };

                // Validar que el correo sea único si se proporciona y es diferente
                if (!string.IsNullOrWhiteSpace(updateClienteDto.Email) && 
                    updateClienteDto.Email != cliente.Email)
                {
                    bool isEmailUnique = await _emailValidationService.IsEmailUniqueAsync(updateClienteDto.Email, excludeClienteId: id);
                    if (!isEmailUnique)
                    {
                        string? owner = await _emailValidationService.GetEmailOwnerAsync(updateClienteDto.Email);
                        return BadRequest(new { message = $"El correo ya está registrado por: {owner}" });
                    }
                }

                // Actualizar campos
                cliente.Nombre = updateClienteDto.Nombre ?? cliente.Nombre;
                cliente.Apellido = updateClienteDto.Apellido ?? cliente.Apellido;
                cliente.Email = updateClienteDto.Email ?? cliente.Email;
                cliente.Telefono = updateClienteDto.Telefono ?? cliente.Telefono;
                cliente.Direccion = updateClienteDto.Direccion ?? cliente.Direccion;

                if (updateClienteDto.Activo.HasValue)
                {
                    cliente.Activo = updateClienteDto.Activo.Value;
                }

                // Actualizar la fecha de eliminación (puede ser null para restaurar)
                cliente.FechaEliminacion = updateClienteDto.FechaEliminacion;

                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                    var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                    var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                    var usuarioNombre = $"{nombre} {apellido}".Trim();

                    var camposActualizados = new List<string>();
                    if (clienteOriginal.Nombre != cliente.Nombre) camposActualizados.Add($"Nombre: {clienteOriginal.Nombre} → {cliente.Nombre}");
                    if (clienteOriginal.Apellido != cliente.Apellido) camposActualizados.Add($"Apellido: {clienteOriginal.Apellido} → {cliente.Apellido}");
                    if (clienteOriginal.Email != cliente.Email) camposActualizados.Add($"Email: {clienteOriginal.Email} → {cliente.Email}");
                    if (clienteOriginal.Telefono != cliente.Telefono) camposActualizados.Add($"Teléfono: {clienteOriginal.Telefono} → {cliente.Telefono}");
                    if (clienteOriginal.Direccion != cliente.Direccion) camposActualizados.Add($"Dirección: {clienteOriginal.Direccion} → {cliente.Direccion}");
                    if (clienteOriginal.Activo != cliente.Activo) camposActualizados.Add($"Estado: {(clienteOriginal.Activo ? "Activo" : "Inactivo")} → {(cliente.Activo ? "Activo" : "Inactivo")}");

                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = usuarioId,
                        NombreUsuario = usuarioNombre,
                        TipoAccion = "Editar",
                        Modulo = "Clientes",
                        Descripcion = $"Cliente '{cliente.Nombre} {cliente.Apellido}' actualizado: {(camposActualizados.Count > 0 ? string.Join(", ", camposActualizados) : "Sin cambios significativos")}",
                        RegistroAfectadoId = cliente.Id,
                        RegistroAfectadoDescripcion = $"Cliente: {cliente.Nombre} {cliente.Apellido}"
                    });
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning($"⚠️ No se pudo registrar auditoría: {auditEx.Message}");
                }

                // Detectar cambios y enviar email si hay modificaciones
                var camposModificados = new List<string>();
                if (clienteOriginal.Nombre != cliente.Nombre)
                    camposModificados.Add($"<li>Nombre: {clienteOriginal.Nombre} → {cliente.Nombre}</li>");
                if (clienteOriginal.Apellido != cliente.Apellido)
                    camposModificados.Add($"<li>Apellido: {clienteOriginal.Apellido} → {cliente.Apellido}</li>");
                if (clienteOriginal.Email != cliente.Email)
                    camposModificados.Add($"<li>Email: {clienteOriginal.Email} → {cliente.Email}</li>");
                if (clienteOriginal.Telefono != cliente.Telefono)
                    camposModificados.Add($"<li>Teléfono: {clienteOriginal.Telefono} → {cliente.Telefono}</li>");
                if (clienteOriginal.Direccion != cliente.Direccion)
                    camposModificados.Add($"<li>Dirección: {clienteOriginal.Direccion} → {cliente.Direccion}</li>");
                if (clienteOriginal.Activo != cliente.Activo)
                    camposModificados.Add($"<li>Estado: {(clienteOriginal.Activo ? "Activo" : "Inactivo")} → {(cliente.Activo ? "Activo" : "Inactivo")}</li>");

                // Enviar email solo si hay cambios
                // COMENTADO: Solo queremos correos al crear/eliminar facturas
                //if (camposModificados.Count > 0 && !string.IsNullOrWhiteSpace(cliente.Email))
                //{
                //    try
                //    {
                //        string clienteCompleto = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                //        string camposHTML = string.Join("\n", camposModificados);
                //        await _emailService.SendClienteUpdatedEmailAsync(cliente.Email, clienteCompleto, camposHTML);
                //        _logger.LogInformation($"✅ Email de actualización enviado a {cliente.Email}");
                //    }
                //    catch (Exception emailEx)
                //    {
                //        _logger.LogWarning($"⚠️ No se pudo enviar email de actualización a {cliente.Email}: {emailEx.Message}");
                //    }
                //}

                var result = new ClienteResponseDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Documento = cliente.Documento,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Activo = cliente.Activo,
                    FechaCreacion = cliente.FechaCreacion,
                    FechaEliminacion = cliente.FechaEliminacion
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Busca clientes por término (nombre, documento, correo)
        /// </summary>
        [HttpGet("buscar/{termino}")]
        public async Task<ActionResult<List<ClienteResponseDto>>> BuscarClientes(string termino)
        {
            try
            {
                var clientes = await _unitOfWork.Clientes.SearchAsync(termino);
                var result = clientes
                    .Select(c => new ClienteResponseDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Documento = c.Documento,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Activo = c.Activo,
                        FechaCreacion = c.FechaCreacion,
                        FechaEliminacion = c.FechaEliminacion
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un cliente
        /// </summary>
        [HttpPut("{id}/desactivar")]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<ActionResult<bool>> DesactivarCliente(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new { message = $"Cliente con ID {id} no encontrado" });
                }

                cliente.Activo = false;
                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un email es único en el sistema (clientes y usuarios)
        /// </summary>
        [HttpPost("check-email")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CheckEmailUnique([FromBody] EmailCheckRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Email))
                {
                    return BadRequest(new { message = "El email es requerido" });
                }

                var isUnique = await _emailValidationService.IsEmailUniqueAsync(
                    request.Email, 
                    excludeClienteId: request.ExcludeId
                );

                if (isUnique)
                {
                    return Ok(new { isUnique = true });
                }

                var owner = await _emailValidationService.GetEmailOwnerAsync(request.Email);
                return Ok(new 
                { 
                    isUnique = false, 
                    owner = owner 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un cliente
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<ActionResult> DeleteCliente(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new { message = $"Cliente con ID {id} no encontrado" });
                }

                cliente.Activo = false;
                cliente.FechaEliminacion = DateTime.UtcNow;
                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                    var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                    var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                    var usuarioNombre = $"{nombre} {apellido}".Trim();
                    
                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = usuarioId,
                        NombreUsuario = usuarioNombre,
                        TipoAccion = "Eliminar",
                        Modulo = "Clientes",
                        Descripcion = $"Cliente '{cliente.Nombre} {cliente.Apellido}' eliminado (desactivado)",
                        RegistroAfectadoId = id,
                        RegistroAfectadoDescripcion = $"Cliente: {cliente.Nombre} {cliente.Apellido} (Documento: {cliente.Documento})",
                        DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });
                    _logger.LogInformation($"✅ Auditoría registrada para eliminación de cliente {id}");
                }
                catch (Exception auditEx)
                {
                    _logger.LogWarning($"⚠️ No se pudo registrar auditoría: {auditEx.Message}");
                }

                // Enviar email de notificación de desactivación
                // COMENTADO: Solo queremos correos al crear/eliminar facturas
                //if (!string.IsNullOrWhiteSpace(cliente.Email))
                //{
                //    try
                //    {
                //        string clienteCompleto = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                //        await _emailService.SendClienteDeletedEmailAsync(cliente.Email, clienteCompleto, cliente.Documento);
                //        _logger.LogInformation($"✅ Email de desactivación enviado a {cliente.Email}");
                //    }
                //    catch (Exception emailEx)
                //    {
                //        _logger.LogWarning($"⚠️ No se pudo enviar email de desactivación a {cliente.Email}: {emailEx.Message}");
                //    }
                //}

                return Ok(new { message = "Cliente desactivado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo para verificar disponibilidad de email
    /// </summary>
    public class EmailCheckRequest
    {
        public string? Email { get; set; }
        public int? ExcludeId { get; set; }
    }
}
