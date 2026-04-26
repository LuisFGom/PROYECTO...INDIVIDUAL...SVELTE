using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure.Persistencia;

namespace PuntoVenta.Infrastructure.Repositories
{
    public class AuditoriaAccionRepository : GenericRepository<AuditoriaAccion>, IAuditoriaAccionRepository
    {
        public AuditoriaAccionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<AuditoriaAccion>> GetByUsuarioIdAsync(string usuarioId)
        {
            return await _context.AuditoriasAcciones
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.FechaAccion)
                .ToListAsync();
        }

        public async Task<List<AuditoriaAccion>> GetByModuloAsync(string modulo)
        {
            return await _context.AuditoriasAcciones
                .Where(a => a.Modulo == modulo)
                .OrderByDescending(a => a.FechaAccion)
                .ToListAsync();
        }

        public async Task<List<AuditoriaAccion>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.AuditoriasAcciones
                .Where(a => a.FechaAccion >= fechaInicio && a.FechaAccion <= fechaFin)
                .OrderByDescending(a => a.FechaAccion)
                .ToListAsync();
        }

        public async Task<List<AuditoriaAccion>> GetAccionesWithFiltersAsync(
            string? usuarioId = null,
            string? modulo = null,
            string? tipoAccion = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int skip = 0,
            int take = 50)
        {
            var query = _context.AuditoriasAcciones.AsQueryable();

            if (!string.IsNullOrEmpty(usuarioId))
            {
                query = query.Where(a => a.UsuarioId == usuarioId);
            }

            if (!string.IsNullOrEmpty(modulo))
            {
                query = query.Where(a => a.Modulo == modulo);
            }

            if (!string.IsNullOrEmpty(tipoAccion))
            {
                query = query.Where(a => a.TipoAccion == tipoAccion);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(a => a.FechaAccion >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(a => a.FechaAccion <= fechaFin.Value);
            }

            return await query
                .OrderByDescending(a => a.FechaAccion)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<AuditoriaAccion> RegistrarAccionAsync(AuditoriaAccion accion)
        {
            accion.FechaAccion = DateTime.UtcNow;
            await AddAsync(accion);
            return accion;
        }
    }
}
