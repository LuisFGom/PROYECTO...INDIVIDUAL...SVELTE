using MediatR;
using PuntoVenta.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class ReinsertatVentaCommandHandler : IRequestHandler<ReinsertatVentaCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReinsertatVentaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ReinsertatVentaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var factura = await _unitOfWork.Facturas.GetFacturaConDetallesAsync(request.VentaId);

                if (factura == null)
                {
                    throw new Exception($"Factura con ID {request.VentaId} no encontrada");
                }

                if (factura.Estado != "Eliminada")
                {
                    throw new Exception("Solo se pueden reinsertar facturas que estén marcadas como eliminadas");
                }

                // Marcar factura como completada/activa
                factura.Estado = "Completada";

                // Restar stock de todos los productos de esta factura (ya que se está reactivando)
                foreach (var detalle in factura.Detalles)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        if (producto.Stock < detalle.Cantidad)
                        {
                            throw new Exception($"Stock insuficiente para reinsertar. Producto: {producto.Nombre}");
                        }
                        producto.Stock -= detalle.Cantidad;
                        await _unitOfWork.Productos.UpdateAsync(producto);
                    }
                }

                await _unitOfWork.Facturas.UpdateAsync(factura);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al reinsertar factura: {ex.Message}");
            }
        }
    }
}
