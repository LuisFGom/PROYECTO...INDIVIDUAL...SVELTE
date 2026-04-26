using MediatR;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el producto existente
                var producto = await _unitOfWork.Productos.GetByIdAsync(request.Id);
                if (producto == null)
                {
                    throw new Exception("Producto no encontrado");
                }

                // Actualizar campos
                if (!string.IsNullOrEmpty(request.Nombre))
                    producto.Nombre = request.Nombre;

                if (!string.IsNullOrEmpty(request.Descripcion))
                    producto.Descripcion = request.Descripcion;

                if (request.PrecioVenta > 0)
                    producto.Precio = request.PrecioVenta;

                if (request.PrecioCompra > 0)
                    producto.PrecioCompra = request.PrecioCompra;

                if (request.Stock.HasValue)
                    producto.Stock = request.Stock.Value;

                if (request.StockMinimo.HasValue)
                    producto.StockMinimo = request.StockMinimo.Value;

                if (request.PorcentajeIVA.HasValue)
                    producto.PorcentajeIVA = request.PorcentajeIVA.Value;

                producto.FechaActualizacion = DateTime.UtcNow;

                await _unitOfWork.Productos.UpdateAsync(producto);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = request.UsuarioId ?? "desconocido",
                        NombreUsuario = request.NombreUsuario ?? "Sistema",
                        TipoAccion = "Editar",
                        Modulo = "Productos",
                        Descripcion = $"Producto '{producto.Nombre}' actualizado",
                        RegistroAfectadoId = producto.Id,
                        RegistroAfectadoDescripcion = $"Producto: {producto.Nombre}"
                    });
                }
                catch (Exception auditEx)
                {
                    System.Console.WriteLine($"Advertencia: No se pudo registrar auditoría: {auditEx.Message}");
                }

                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el producto: {ex.Message}");
            }
        }
    }
}
