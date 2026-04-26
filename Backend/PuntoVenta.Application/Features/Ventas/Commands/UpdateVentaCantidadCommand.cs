using MediatR;
using System.Collections.Generic;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class UpdateVentaCantidadCommand : IRequest<bool>
    {
        public int VentaId { get; set; }
        public List<UpdateCantidadDetalleDto> Detalles { get; set; } = new();
    }

    public class UpdateCantidadDetalleDto
    {
        public int DetalleId { get; set; }
        public int NuevaCantidad { get; set; }
    }
}
