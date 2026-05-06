using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Features.Products.Commands.CreateProduct;
using PuntoVenta.Application.Features.Products.Commands.UpdateProduct;
using PuntoVenta.Application.Features.Products.Commands.DeleteProduct;
using PuntoVenta.Application.Features.Products.Queries.GetProducts;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;


namespace PuntoVenta.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IMediator mediator, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _unitOfWork.Productos.GetAllAsync();
            return Ok(productos);
        }

        [Authorize]
        [HttpGet("eliminados")]
        public async Task<IActionResult> GetEliminados()
        {
            var productos = await _unitOfWork.Productos.GetAllAsync();
            var eliminados = productos
                .Where(p => !p.Activo)
                .Select(p => new {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.PrecioCompra,
                    p.Precio,
                    p.Stock,
                    p.StockMinimo,
                    p.Descripcion,
                    p.PorcentajeIVA,
                    p.Activo,
                    p.FechaCreacion,
                    p.FechaActualizacion
                })
                .ToList();
            return Ok(eliminados);
        }

        [Authorize(Roles = "Administrador,Admin")]
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] CreateProductCommand command)
        {
            try
            {
                // Agregar información del usuario
                command.UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                command.NombreUsuario = $"{nombre} {apellido}".Trim();

                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(CrearProducto), new { id }, new { Message = $"Producto creado con ID: {id}" });
            }
            catch (Exception ex)
            {
                // Verificar si es error de código duplicado
                if (ex.Message.Contains("Ya existe un producto con este código") || 
                    ex.Message.Contains("código"))
                {
                    return Conflict(new { message = "Ya existe un producto con este código de barras" });
                }
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id) return BadRequest();

            // Agregar información del usuario
            command.UsuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
            var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
            var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
            command.NombreUsuario = $"{nombre} {apellido}".Trim();
            
            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize(Roles = "Administrador,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id, [FromQuery] string? motivo = null)
        {
            try
            {
                // Obtener el producto antes de eliminarlo
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);
                if (producto == null)
                {
                    return NotFound(new { mensaje = "Producto no encontrado" });
                }

                // Obtener información del administrador
                var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var adminIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                var adminNombre = $"{nombre} {apellido}".Trim();
                if (string.IsNullOrWhiteSpace(adminNombre)) adminNombre = "Sistema";

                // Registrar la eliminación
                var eliminacion = new EliminacionProducto
                {
                    ProductoEliminadoId = producto.Id,
                    CodigoProductoEliminado = producto.Codigo,
                    NombreProductoEliminado = producto.Nombre,
                    DescripcionProductoEliminado = producto.Descripcion,
                    PrecioVentaProductoEliminado = producto.Precio,
                    PrecioCostoProductoEliminado = producto.PrecioCompra,
                    StockProductoEliminado = producto.Stock,
                    AdministradorId = adminId,
                    NombreAdministrador = adminNombre,
                    FechaEliminacion = DateTime.UtcNow,
                    MotivoEliminacion = motivo,
                    TipoEliminacion = "Desactivación",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                await _unitOfWork.EliminacionesProductos.AddAsync(eliminacion);

                // Desactivar el producto
                producto.Activo = false;
                producto.FechaActualizacion = DateTime.UtcNow;
                await _unitOfWork.Productos.UpdateAsync(producto);

                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = adminIdStr,
                        NombreUsuario = adminNombre,
                        TipoAccion = "Eliminar",
                        Modulo = "Productos",
                        Descripcion = $"Producto '{producto.Nombre}' eliminado (desactivado). Motivo: {(motivo ?? "No especificado")}",
                        RegistroAfectadoId = producto.Id,
                        RegistroAfectadoDescripcion = $"Producto: {producto.Nombre} (Código: {producto.Codigo})",
                        DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });
                }
                catch (Exception auditEx)
                {
                    // Log pero no fallar
                    System.Console.WriteLine($"Advertencia: No se pudo registrar auditoría: {auditEx.Message}");
                }

                return Ok(new { mensaje = "Producto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador,Admin")]
        [HttpPut("{id}/restaurar")]
        public async Task<IActionResult> RestaurarProducto(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null)
            {
                return NotFound(new { mensaje = "Producto no encontrado" });
            }
            if (producto.Activo)
            {
                return BadRequest(new { mensaje = "El producto ya está activo" });
            }
            producto.Activo = true;
            await _unitOfWork.Productos.UpdateAsync(producto);
            
            // Registrar en auditoría
            var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string usuarioId = usuarioIdClaim?.Value ?? "0";
            var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
            var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
            string nombreUsuario = $"{nombre} {apellido}".Trim();
            
            var auditoria = new AuditoriaAccion
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                TipoAccion = "Editar",
                Modulo = "Productos",
                Descripcion = $"Producto '{producto.Nombre}' ha sido restaurado y está nuevamente disponible en el sistema",
                RegistroAfectadoId = id,
                RegistroAfectadoDescripcion = producto.Nombre,
                FechaAccion = DateTime.UtcNow
            };
            await _unitOfWork.AuditoriasAcciones.AddAsync(auditoria);
            
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { mensaje = "Producto restaurado correctamente" });
        }
    }
}
