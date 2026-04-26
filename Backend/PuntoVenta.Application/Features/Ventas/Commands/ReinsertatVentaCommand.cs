using MediatR;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class ReinsertatVentaCommand : IRequest<bool>
    {
        public int VentaId { get; set; }
    }
}
