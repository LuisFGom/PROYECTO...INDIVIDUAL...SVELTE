namespace PuntoVenta.Application.DTOs
{
    /// <summary>
    /// DTO para respuesta de acciones de auditoría
    /// </summary>
    public class AuditoriaAccionResponseDto
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string TipoAccion { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int? RegistroAfectadoId { get; set; }
        public string? RegistroAfectadoDescripcion { get; set; }
        public DateTime FechaAccion { get; set; }
        public string? DireccionIP { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
    }
}
