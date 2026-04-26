using MediatR;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace PuntoVenta.Application.Features.Usuarios.Commands
{
    public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUsuarioCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Normalizar datos (trim y lowercase para emails)
                var nombreUsuario = (request.NombreUsuario ?? string.Empty).Trim();
                var email = (request.Email ?? string.Empty).Trim().ToLower();
                var nombre = (request.Nombre ?? string.Empty).Trim();
                var apellido = (request.Apellido ?? string.Empty).Trim();

                // Validar que los campos no estén vacíos
                if (string.IsNullOrEmpty(nombreUsuario))
                    throw new Exception("El nombre de usuario no puede estar vacío");
                if (string.IsNullOrEmpty(email))
                    throw new Exception("El email no puede estar vacío");
                if (string.IsNullOrEmpty(nombre))
                    throw new Exception("El nombre no puede estar vacío");
                if (string.IsNullOrEmpty(apellido))
                    throw new Exception("El apellido no puede estar vacío");

                // Validar que no exista usuario con el mismo email
                if (await _unitOfWork.Usuarios.ExisteCorreoAsync(email))
                {
                    throw new Exception($"Ya existe un usuario registrado con el email: {email}");
                }

                // Validar que no exista usuario con el mismo nombre de usuario
                if (await _unitOfWork.Usuarios.ExisteCedulaAsync(nombreUsuario))
                {
                    throw new Exception($"Ya existe un usuario registrado con el nombre: {nombreUsuario}");
                }

                // Verificar que el rol existe
                var rol = await _unitOfWork.Roles.GetByIdAsync(request.RolId);
                if (rol == null)
                {
                    throw new Exception($"El rol con ID {request.RolId} no existe");
                }

                // Hash de contraseña usando BCrypt
                var contraseniaHasheada = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Contrasena, HashType.SHA384);

                // Crear nuevo usuario
                var nuevoUsuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Email = email,
                    Nombre = nombre,
                    Apellido = apellido,
                    PasswordHash = contraseniaHasheada,
                    RolId = request.RolId,
                    RolNombre = rol.Nombre,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Usuarios.AddAsync(nuevoUsuario);
                await _unitOfWork.SaveChangesAsync();

                return nuevoUsuario.Id;
            }
            catch (Exception ex)
            {
                // Re-lanzar con contexto más claro
                throw new Exception($"Error al crear el usuario: {ex.Message}");
            }
        }
    }
}
