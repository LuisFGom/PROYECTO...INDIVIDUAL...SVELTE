using PuntoVenta.Domain.Entities;

namespace PuntoVenta.Application.Interfaces
{
    /// <summary>
    /// Interfaz para operaciones de auditoría de acciones
    /// </summary>
    public interface IAuditoriaAccionRepository : IGenericRepository<AuditoriaAccion>
    {
        /// <summary>
        /// Obtiene las acciones de un usuario específico
        /// </summary>
        Task<List<AuditoriaAccion>> GetByUsuarioIdAsync(string usuarioId);

        /// <summary>
        /// Obtiene las acciones de un módulo específico
        /// </summary>
        Task<List<AuditoriaAccion>> GetByModuloAsync(string modulo);

        /// <summary>
        /// Obtiene acciones dentro de un rango de fechas
        /// </summary>
        Task<List<AuditoriaAccion>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene todas las acciones con paginación y filtros
        /// </summary>
        Task<List<AuditoriaAccion>> GetAccionesWithFiltersAsync(
            string? usuarioId = null,
            string? modulo = null,
            string? tipoAccion = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int skip = 0,
            int take = 50);

        /// <summary>
        /// Registra una nueva acción de auditoría
        /// </summary>
        Task<AuditoriaAccion> RegistrarAccionAsync(AuditoriaAccion accion);
    }
}
