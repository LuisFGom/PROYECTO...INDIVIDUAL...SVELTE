using MediatR;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class DeleteVentaSoftCommand : IRequest<bool>
    {
        public int VentaId { get; set; }
        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
    }
}
