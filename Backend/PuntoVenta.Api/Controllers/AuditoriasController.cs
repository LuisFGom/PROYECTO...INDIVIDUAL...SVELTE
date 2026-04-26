using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.DTOs;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Features.Auditorias.Queries;

namespace PuntoVenta.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditoriasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuditoriasController> _logger;

        public AuditoriasController(IMediator mediator, ILogger<AuditoriasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las acciones de auditoría con filtros opcionales
        /// </summary>
        [HttpGet("acciones")]
        public async Task<ActionResult<List<AuditoriaAccionResponseDto>>> GetAcciones(
            [FromQuery] string? usuarioId = null,
            [FromQuery] string? modulo = null,
            [FromQuery] string? tipoAccion = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                _logger.LogInformation("[AUDITORIAS] Obteniendo acciones con filtros");
                
                var query = new GetAuditoriasAccionesQuery
                {
                    UsuarioId = usuarioId,
                    Modulo = modulo,
                    TipoAccion = tipoAccion,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    Skip = skip,
                    Take = take
                };

                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUDITORIAS] Error al obtener acciones");
                return StatusCode(500, new { message = "Error al obtener acciones de auditoría" });
            }
        }

        /// <summary>
        /// Obtiene todas las acciones de un usuario específico
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<AuditoriaAccionResponseDto>>> GetAccionesPorUsuario(string usuarioId)
        {
            try
            {
                _logger.LogInformation($"[AUDITORIAS] Obteniendo acciones del usuario: {usuarioId}");
                
                var query = new GetAuditoriasAccionesQuery
                {
                    UsuarioId = usuarioId,
                    Take = 100
                };

                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AUDITORIAS] Error al obtener acciones del usuario {usuarioId}");
                return StatusCode(500, new { message = "Error al obtener acciones de auditoría" });
            }
        }

        /// <summary>
        /// Obtiene todas las acciones de un módulo específico
        /// </summary>
        [HttpGet("modulo/{modulo}")]
        public async Task<ActionResult<List<AuditoriaAccionResponseDto>>> GetAccionesPorModulo(string modulo)
        {
            try
            {
                _logger.LogInformation($"[AUDITORIAS] Obteniendo acciones del módulo: {modulo}");
                
                var query = new GetAuditoriasAccionesQuery
                {
                    Modulo = modulo,
                    Take = 100
                };

                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AUDITORIAS] Error al obtener acciones del módulo {modulo}");
                return StatusCode(500, new { message = "Error al obtener acciones de auditoría" });
            }
        }

        /// <summary>
        /// Registra una nueva acción de auditoría
        /// </summary>
        [HttpPost("registrar")]
        public async Task<ActionResult<int>> RegistrarAccion([FromBody] RegistrarAuditoriaAccionCommand command)
        {
            try
            {
                _logger.LogInformation($"[AUDITORIAS] Registrando acción: {command.TipoAccion} en {command.Modulo}");
                
                var resultado = await _mediator.Send(command);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUDITORIAS] Error al registrar acción");
                return StatusCode(500, new { message = "Error al registrar acción de auditoría" });
            }
        }
    }
}
