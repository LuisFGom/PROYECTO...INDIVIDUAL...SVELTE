using MediatR;
using PuntoVenta.Application.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Ventas.Commands
{
    public class UpdateVentaCantidadCommandHandler : IRequestHandler<UpdateVentaCantidadCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVentaCantidadCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateVentaCantidadCommand request, CancellationToken cancellationToken)
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
                    throw new Exception("No se puede editar una factura eliminada");
                }

                // Validar que solo haya detalles para actualizar
                if (request.Detalles == null || request.Detalles.Count == 0)
                {
                    throw new Exception("Debe proporcionar detalles para actualizar");
                }

                decimal diferenciaNueva = 0;

                foreach (var detalleUpdate in request.Detalles)
                {
                    var detalle = factura.Detalles.FirstOrDefault(d => d.Id == detalleUpdate.DetalleId);
                    
                    if (detalle == null)
                    {
                        throw new Exception($"Detalle con ID {detalleUpdate.DetalleId} no encontrado");
                    }

                    if (detalleUpdate.NuevaCantidad <= 0)
                    {
                        throw new Exception("La cantidad debe ser mayor a 0");
                    }

                    // Calcular diferencia de cantidad
                    int diferenciaCantidad = detalleUpdate.NuevaCantidad - detalle.Cantidad;

                    // Calcular diferencia de precio
                    decimal diferenciaPrecio = detalle.PrecioUnitario * diferenciaCantidad;

                    // Aplicar descuento si existe
                    if (detalle.Descuento > 0)
                    {
                        diferenciaPrecio = diferenciaPrecio * (1 - (detalle.Descuento / 100m));
                    }

                    // Actualizar cantidad y total del detalle
                    detalle.Cantidad = detalleUpdate.NuevaCantidad;
                    detalle.Total = (detalle.PrecioUnitario * detalleUpdate.NuevaCantidad) * (1 - (detalle.Descuento / 100m));

                    diferenciaNueva += diferenciaPrecio;

                    // Actualizar stock del producto (si disminuyó cantidad, aumentar stock; si aumentó, disminuir)
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= diferenciaCantidad;
                        if (producto.Stock < 0)
                        {
                            throw new Exception($"Stock insuficiente para el producto {producto.Nombre}");
                        }
                        await _unitOfWork.Productos.UpdateAsync(producto);
                    }
                }

                // Recalcular totales de la factura
                factura.Subtotal = factura.Detalles.Sum(d => d.Total);
                factura.TotalImpuesto = factura.Subtotal * (factura.PorcentajeIVA / 100m);
                factura.TotalVenta = factura.Subtotal + factura.TotalImpuesto;

                await _unitOfWork.Facturas.UpdateAsync(factura);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar cantidades de factura: {ex.Message}");
            }
        }
    }
}
