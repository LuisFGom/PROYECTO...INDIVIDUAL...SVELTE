using MediatR;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Application.Features.Auditorias.Handlers
{
    public class RegistrarAuditoriaAccionCommandHandler : IRequestHandler<RegistrarAuditoriaAccionCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarAuditoriaAccionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(RegistrarAuditoriaAccionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var auditoria = new AuditoriaAccion
                {
                    UsuarioId = request.UsuarioId,
                    NombreUsuario = request.NombreUsuario,
                    TipoAccion = request.TipoAccion,
                    Modulo = request.Modulo,
                    Descripcion = request.Descripcion,
                    RegistroAfectadoId = request.RegistroAfectadoId,
                    RegistroAfectadoDescripcion = request.RegistroAfectadoDescripcion,
                    DireccionIP = request.DireccionIP,
                    DatosAnteriores = request.DatosAnteriores,
                    DatosNuevos = request.DatosNuevos,
                    FechaAccion = DateTime.UtcNow
                };

                await _unitOfWork.AuditoriasAcciones.AddAsync(auditoria);
                await _unitOfWork.SaveChangesAsync();

                return auditoria.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RegistrarAuditoriaAccionCommandHandler: {ex.Message}");
                throw;
            }
        }
    }
}
