using MediatR;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Features.Auditorias.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class DeleteVentaSoftCommandHandler : IRequestHandler<DeleteVentaSoftCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public DeleteVentaSoftCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVentaSoftCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var factura = await _unitOfWork.Facturas.GetFacturaConDetallesAsync(request.VentaId);

                if (factura == null)
                {
                    throw new Exception($"Factura con ID {request.VentaId} no encontrada");
                }

                if (factura.Estado == "Eliminada")
                {
                    throw new Exception("Esta factura ya está marcada como eliminada");
                }

                // Marcar factura como eliminada (soft delete)
                factura.Estado = "Eliminada";

                // Restaurar stock de todos los productos de esta factura
                foreach (var detalle in factura.Detalles)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock += detalle.Cantidad;
                        await _unitOfWork.Productos.UpdateAsync(producto);
                    }
                }

                await _unitOfWork.Facturas.UpdateAsync(factura);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = request.UsuarioId ?? "desconocido",
                        NombreUsuario = request.NombreUsuario ?? "Sistema",
                        TipoAccion = "Eliminar",
                        Modulo = "Ventas",
                        Descripcion = $"Factura '{factura.NumeroFactura}' marcada como eliminada",
                        RegistroAfectadoId = factura.Id,
                        RegistroAfectadoDescripcion = $"Factura: {factura.NumeroFactura}"
                    });
                }
                catch (Exception auditEx)
                {
                    // Log pero no fallar
                    System.Console.WriteLine($"Advertencia: No se pudo registrar auditoría: {auditEx.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar factura: {ex.Message}");
            }
        }
    }
}
