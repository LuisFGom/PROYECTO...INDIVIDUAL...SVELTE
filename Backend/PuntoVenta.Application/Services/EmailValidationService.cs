using System.Threading.Tasks;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Application.Services
{
    /// <summary>
    /// Servicio para validar la unicidad de correos entre usuarios y clientes
    /// </summary>
    public interface IEmailValidationService
    {
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null, int? excludeClienteId = null);
        Task<string?> GetEmailOwnerAsync(string email);
    }

    public class EmailValidationService : IEmailValidationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailValidationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Verifica si un correo es único en toda la aplicación
        /// </summary>
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null, int? excludeClienteId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.ToLower().Trim();

            // Verificar en Usuarios
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuarioExists = usuarios.Any(u =>
                u.Email.ToLower() == email &&
                (excludeUserId == null || u.Id != excludeUserId));

            if (usuarioExists)
                return false;

            // Verificar en Clientes
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            var clienteExists = clientes.Any(c =>
                !string.IsNullOrWhiteSpace(c.Email) &&
                c.Email.ToLower() == email &&
                (excludeClienteId == null || c.Id != excludeClienteId));

            return !clienteExists;
        }

        /// <summary>
        /// Obtiene quién es propietario de un correo (Usuario o Cliente)
        /// </summary>
        public async Task<string?> GetEmailOwnerAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.ToLower().Trim();

            // Buscar en Usuarios
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Email.ToLower() == email);
            if (usuario != null)
                return $"Usuario: {usuario.NombreUsuario}";

            // Buscar en Clientes
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            var cliente = clientes.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Email) &&
                c.Email.ToLower() == email);

            if (cliente != null)
                return $"Cliente: {cliente.Nombre} {cliente.Apellido}";

            return null;
        }
    }
}
