using MediatR;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que no exista producto con el mismo código
                var productoExistente = await _unitOfWork.Productos.GetByCodigoBarraAsync(request.Codigo ?? string.Empty);
                if (productoExistente != null)
                {
                    throw new Exception("Ya existe un producto con este código");
                }

                // Crear nuevo producto
                var nuevoProducto = new Product
                {
                    Nombre = request.Nombre ?? string.Empty,
                    Codigo = request.Codigo ?? string.Empty,
                    Descripcion = request.Descripcion ?? string.Empty,
                    Precio = request.Precio,
                    PrecioCompra = request.PrecioCompra,
                    Stock = request.StockInicial,
                    StockMinimo = request.StockMinimo > 0 ? request.StockMinimo : 10,
                    PorcentajeIVA = request.PorcentajeIVA,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Productos.AddAsync(nuevoProducto);
                await _unitOfWork.SaveChangesAsync();

                // Registrar auditoría
                try
                {
                    await _mediator.Send(new RegistrarAuditoriaAccionCommand
                    {
                        UsuarioId = request.UsuarioId ?? "desconocido",
                        NombreUsuario = request.NombreUsuario ?? "Sistema",
                        TipoAccion = "Crear",
                        Modulo = "Productos",
                        Descripcion = $"Nuevo producto '{nuevoProducto.Nombre}' creado (Código: {nuevoProducto.Codigo})",
                        RegistroAfectadoId = nuevoProducto.Id,
                        RegistroAfectadoDescripcion = $"Producto: {nuevoProducto.Nombre}"
                    });
                }
                catch (Exception auditEx)
                {
                    System.Console.WriteLine($"Advertencia: No se pudo registrar auditoría: {auditEx.Message}");
                }

                return nuevoProducto.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el producto: {ex.Message}");
            }
        }
    }
}
