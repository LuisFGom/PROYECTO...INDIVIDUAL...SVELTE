using MediatR;
using PuntoVenta.Application.DTOs;
using PuntoVenta.Application.Features.Auditorias.Queries;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Features.Auditorias.Handlers
{
    public class GetAuditoriasAccionesQueryHandler : IRequestHandler<GetAuditoriasAccionesQuery, List<AuditoriaAccionResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAuditoriasAccionesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AuditoriaAccionResponseDto>> Handle(GetAuditoriasAccionesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener acciones con filtros
                var acciones = await _unitOfWork.AuditoriasAcciones.GetAccionesWithFiltersAsync(
                    usuarioId: request.UsuarioId,
                    modulo: request.Modulo,
                    tipoAccion: request.TipoAccion,
                    fechaInicio: request.FechaInicio,
                    fechaFin: request.FechaFin,
                    skip: request.Skip,
                    take: request.Take
                );

                // Mapear a DTO
                var resultado = acciones.Select(a => new AuditoriaAccionResponseDto
                {
                    Id = a.Id,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = a.NombreUsuario,
                    TipoAccion = a.TipoAccion,
                    Modulo = a.Modulo,
                    Descripcion = a.Descripcion,
                    RegistroAfectadoId = a.RegistroAfectadoId,
                    RegistroAfectadoDescripcion = a.RegistroAfectadoDescripcion,
                    FechaAccion = a.FechaAccion,
                    DireccionIP = a.DireccionIP,
                    DatosAnteriores = a.DatosAnteriores,
                    DatosNuevos = a.DatosNuevos
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAuditoriasAccionesQueryHandler: {ex.Message}");
                throw;
            }
        }
    }
}
