namespace PuntoVenta.Domain.Entities
{
    /// <summary>
    /// Entidad para registrar todas las acciones realizadas por usuarios (auditoría)
    /// </summary>
    public class AuditoriaAccion
    {
        public int Id { get; set; }
        
        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;
        
        /// <summary>
        /// Nombre del usuario para fácil visualización
        /// </summary>
        public string NombreUsuario { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de acción (Crear, Editar, Eliminar, Reinsertar, Ver, Descargar, etc.)
        /// </summary>
        public string TipoAccion { get; set; } = string.Empty;
        
        /// <summary>
        /// Módulo donde se realizó la acción (Usuarios, Clientes, Productos, Ventas, Reportes, etc.)
        /// </summary>
        public string Modulo { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción detallada de la acción realizada
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
        
        /// <summary>
        /// ID del registro afectado (ClienteId, ProductoId, VentaId, UsuarioId, etc.)
        /// </summary>
        public int? RegistroAfectadoId { get; set; }
        
        /// <summary>
        /// Descripción del registro afectado (ej: "Factura FAC-001", "Cliente Juan Pérez")
        /// </summary>
        public string? RegistroAfectadoDescripcion { get; set; }
        
        /// <summary>
        /// Fecha y hora cuando se realizó la acción
        /// </summary>
        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Dirección IP del cliente
        /// </summary>
        public string? DireccionIP { get; set; }
        
        /// <summary>
        /// Datos anteriores (para cambios, en JSON)
        /// </summary>
        public string? DatosAnteriores { get; set; }
        
        /// <summary>
        /// Datos nuevos (para cambios, en JSON)
        /// </summary>
        public string? DatosNuevos { get; set; }
    }
}
