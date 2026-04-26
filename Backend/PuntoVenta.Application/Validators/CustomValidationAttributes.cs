using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PuntoVenta.Application.Validators
{
    /// <summary>
    /// Validador personalizado para nombres (solo letras, espacios y acentos)
    /// </summary>
    public class NombreAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // La validación de requerido se hace con [Required]

            string nombre = value.ToString()!.Trim();
            
            // Solo permite letras (incluyendo acentos), espacios y guiones
            // Patrón: Letras mayúsculas, minúsculas, acentuadas, espacios y guiones
            string pattern = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-]+$";
            
            if (!Regex.IsMatch(nombre, pattern))
            {
                ErrorMessage = "El nombre solo puede contener letras, espacios y guiones";
                return false;
            }

            // No debe ser solo espacios
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ErrorMessage = "El nombre no puede estar vacío";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validador para cédula ecuatoriana (10 dígitos con verificación)
    /// </summary>
    public class CedulaEcuatorianaAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true;

            string cedula = value.ToString()!.Trim();

            // Validar que sean exactamente 10 dígitos
            if (!Regex.IsMatch(cedula, @"^\d{10}$"))
            {
                ErrorMessage = "La cédula debe contener exactamente 10 dígitos";
                return false;
            }

            // Algoritmo de validación de cédula ecuatoriana
            return ValidarCedulaEcuatoriana(cedula);
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            try
            {
                // Validación básica: debe ser 10 dígitos numéricos
                if (!Regex.IsMatch(cedula, @"^\d{10}$"))
                {
                    ErrorMessage = "La cédula debe contener exactamente 10 dígitos";
                    return false;
                }

                // 1. Verificar Provincia (primeros 2 dígitos: 01-24)
                int provincia = int.Parse(cedula.Substring(0, 2));
                if (provincia < 1 || provincia > 24)
                {
                    ErrorMessage = "El código de provincia no es válido (debe estar entre 01 y 24)";
                    return false;
                }

                // 2. Verificar Tercer Dígito (0-5 para personas naturales)
                int tercerDigito = int.Parse(cedula.Substring(2, 1));
                if (tercerDigito < 0 || tercerDigito > 5)
                {
                    ErrorMessage = "El tercer dígito debe estar entre 0 y 5";
                    return false;
                }

                // Si pasa todas las validaciones de formato, es válida
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al validar cédula: {ex.Message}";
                return false;
            }
        }
    }

    /// <summary>
    /// Validador para correo único (se valida en el controlador contra la BD)
    /// </summary>
    public class CorreoUnicoAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // La validación de requerido se hace con [Required]

            string email = value.ToString()!.Trim().ToLower();

            // Validación básica de formato
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ErrorMessage = "El formato del correo no es válido";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validador para nombres de producto (solo letras, números y espacios)
    /// </summary>
    public class NombreProductoAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true;

            string nombre = value.ToString()!;

            // Permitir: letras (incluyendo acentos), números, espacios, guiones y paréntesis
            string pattern = @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\-()]+$";
            if (!Regex.IsMatch(nombre, pattern))
            {
                ErrorMessage = "El nombre del producto solo puede contener letras, números, espacios, guiones y paréntesis";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validador para decimales máximo 2 lugares
    /// </summary>
    public class DecimalDosLugaresAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true; // La validación de requerido se hace con [Required]

            if (value is decimal decimalValue)
            {
                // Validar que tenga máximo 2 decimales
                string stringValue = decimalValue.ToString("F20").TrimEnd('0');
                if (stringValue.Contains('.'))
                {
                    int decimalPlaces = stringValue.Length - stringValue.IndexOf('.') - 1;
                    if (decimalPlaces > 2)
                    {
                        ErrorMessage = "El valor debe tener máximo 2 decimales";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

