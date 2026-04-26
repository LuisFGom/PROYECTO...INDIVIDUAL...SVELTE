using MediatR;
using PuntoVenta.Application.DTOs;
using System.Collections.Generic;

namespace PuntoVenta.Application.Features.Ventas.Queries
{
    public class GetEliminacionesVentasQuery : IRequest<List<VentaResponseDto>>
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? ClienteId { get; set; }
        public string? NumeroFactura { get; set; }
    }
}
