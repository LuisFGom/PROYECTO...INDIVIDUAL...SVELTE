using MediatR;
using PuntoVenta.Application.DTOs;
using PuntoVenta.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PuntoVenta.Application.Features.Ventas.Queries
{
    public class GetEliminacionesVentasQueryHandler : IRequestHandler<GetEliminacionesVentasQuery, List<VentaResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEliminacionesVentasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<VentaResponseDto>> Handle(GetEliminacionesVentasQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var facturas = await _unitOfWork.Facturas.GetAllAsync();

                // Filtrar solo las eliminadas
                var query = facturas.Where(f => f.Estado == "Eliminada").AsEnumerable();

                // Aplicar filtros adicionales
                if (request.FechaInicio.HasValue)
                {
                    query = query.Where(f => f.FechaVenta >= request.FechaInicio.Value);
                }

                if (request.FechaFin.HasValue)
                {
                    query = query.Where(f => f.FechaVenta <= request.FechaFin.Value);
                }

                if (request.ClienteId.HasValue)
                {
                    query = query.Where(f => f.ClienteId == request.ClienteId.Value);
                }

                if (!string.IsNullOrEmpty(request.NumeroFactura))
                {
                    query = query.Where(f => f.NumeroFactura.Contains(request.NumeroFactura));
                }

                // Ordenar por fecha descendente
                var resultado = query.OrderByDescending(f => f.FechaVenta)
                    .Select(f => new VentaResponseDto
                    {
                        VentaId = f.Id,
                        NumeroFactura = f.NumeroFactura,
                        FechaVenta = f.FechaVenta,
                        UsuarioId = f.UsuarioId,
                        UsuarioNombre = f.UsuarioNombre,
                        ClienteId = f.ClienteId,
                        ClienteNombre = f.ClienteNombre,
                        Subtotal = f.Subtotal,
                        PorcentajeIVA = f.PorcentajeIVA,
                        TotalImpuesto = f.TotalImpuesto,
                        TotalVenta = f.TotalVenta,
                        Estado = f.Estado,
                        Observaciones = f.Observaciones
                    })
                    .ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener facturas eliminadas: {ex.Message}");
            }
        }
    }
}
