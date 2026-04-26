using MediatR;
using PuntoVenta.Application.DTOs;

namespace PuntoVenta.Application.Features.Auditorias.Queries
{
    /// <summary>
    /// Query para obtener todas las acciones de auditoría con filtros opcionales
    /// </summary>
    public class GetAuditoriasAccionesQuery : IRequest<List<AuditoriaAccionResponseDto>>
    {
        public string? UsuarioId { get; set; }
        public string? Modulo { get; set; }
        public string? TipoAccion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}
