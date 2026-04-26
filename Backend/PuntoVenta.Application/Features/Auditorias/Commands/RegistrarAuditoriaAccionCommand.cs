using MediatR;

namespace PuntoVenta.Application.Features.Auditorias.Commands
{
    /// <summary>
    /// Command para registrar una nueva acción de auditoría
    /// </summary>
    public class RegistrarAuditoriaAccionCommand : IRequest<int>
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string TipoAccion { get; set; } = string.Empty;  // Crear, Editar, Eliminar, Reinsertar, Ver, Descargar, etc.
        public string Modulo { get; set; } = string.Empty;      // Usuarios, Clientes, Productos, Ventas, etc.
        public string Descripcion { get; set; } = string.Empty;
        public int? RegistroAfectadoId { get; set; }
        public string? RegistroAfectadoDescripcion { get; set; }
        public string? DireccionIP { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
    }
}
