using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PuntoVenta.Application.Validators;

namespace PuntoVenta.Application.DTOs
{
    /// <summary>
    /// DTO para Login de usuarios
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 100 caracteres")]
        public string? Contrasena { get; set; }
    }

    /// <summary>
    /// DTO para crear un Usuario
    /// </summary>
    public class CreateUsuarioDto
    {
        [JsonPropertyName("Email")]
        [Required(ErrorMessage = "El email es requerido")]
        [CorreoUnico]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? Email { get; set; }

        [JsonPropertyName("NombreUsuario")]
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string? NombreUsuario { get; set; }

        [JsonPropertyName("Nombre")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [Nombre]
        public string? Nombre { get; set; }

        [JsonPropertyName("Apellido")]
        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [Nombre]
        public string? Apellido { get; set; }

        [JsonPropertyName("Contrasena")]
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
        public string? Contrasena { get; set; }

        // RolId es opcional - si no se proporciona, se usará el rol "defecto"
        public int RolId { get; set; } = 0;
    }

    /// <summary>
    /// DTO para actualizar un Usuario
    /// </summary>
    public class UpdateUsuarioDto
    {
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [Nombre]
        public string? Nombre { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [Nombre]
        public string? Apellido { get; set; }

        [CorreoUnico]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? Email { get; set; }

        public int? RolId { get; set; }

        public bool? Activo { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de Usuario
    /// </summary>
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Cedula { get; set; } // Alias para NombreUsuario (para el frontend)
        public string? Email { get; set; }
        public string? Correo { get; set; } // Alias para Email (para el frontend)
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimoLogin { get; set; }
        public string? RolNombre { get; set; }
        public int RolId { get; set; }
    }

    /// <summary>
    /// DTO flexible para crear usuario desde el frontend
    /// Acepta tanto los nombres del frontend (cedula, nombre, apellido, correo) 
    /// como los del backend (nombreUsuario, nombre, apellido, email)
    /// </summary>
    public class CreateUsuarioRequestDto
    {
        // Campos del backend
        public string? NombreUsuario { get; set; }
        public string? Email { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        // Campos del frontend (aliases)
        [CedulaEcuatoriana(ErrorMessage = "La cédula no es válida")]
        public string? Cedula { get; set; }
        public string? Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string? Contrasena { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }
    }
}
