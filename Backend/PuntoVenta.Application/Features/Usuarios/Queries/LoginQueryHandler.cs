using MediatR;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace PuntoVenta.Application.Features.Usuarios.Queries
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public LoginQueryHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var response = new AuthResponse();

            // Validar que no estén vacíos
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contrasena))
            {
                response.Exitoso = false;
                response.Mensaje = "El email y contraseña son requeridos";
                return response;
            }

            try
            {
                // Obtener usuario por email
                var usuario = await _unitOfWork.Usuarios.GetByCorreoAsync(request.Email);
                if (usuario == null)
                {
                    // Registrar intento fallido
                    await _unitOfWork.IntentosLogin.IncrementarIntentosAsync(request.Email, "", "");
                    await _unitOfWork.SaveChangesAsync();

                    response.Exitoso = false;
                    response.Mensaje = "Email o contraseña incorrectos";
                    return response;
                }

                // Verificar si el usuario está activo
                if (!usuario.Activo)
                {
                    response.Exitoso = false;
                    response.Mensaje = "Usuario Desactivado por favor comuniquese con el Administrador del Sistema";
                    return response;
                }

                // Verificar contraseña (BCrypt)
                bool esContrasenValida = false;
                if (!string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    // Primero intentar con EnhancedVerify (si se guardó con EnhancedHashPassword)
                    try
                    {
                        esContrasenValida = BCrypt.Net.BCrypt.EnhancedVerify(request.Contrasena, usuario.PasswordHash, HashType.SHA384);
                    }
                    catch
                    {
                        // Si EnhancedVerify falla, intentar con Verify normal
                        esContrasenValida = BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.PasswordHash);
                    }
                    
                    // Debug
                    System.Console.WriteLine($"[DEBUG LOGIN] Email: {request.Email}");
                    System.Console.WriteLine($"[DEBUG LOGIN] Password provided: {request.Contrasena}");
                    System.Console.WriteLine($"[DEBUG LOGIN] Password hash exists: {!string.IsNullOrEmpty(usuario.PasswordHash)}");
                    System.Console.WriteLine($"[DEBUG LOGIN] Password hash length: {usuario.PasswordHash?.Length}");
                    System.Console.WriteLine($"[DEBUG LOGIN] Password verification result: {esContrasenValida}");
                }
                
                if (!esContrasenValida)
                {
                    var emailLower = request.Email.ToLower();
                    System.Console.WriteLine($"\n========== [LOGIN FALLIDO] ==========");
                    System.Console.WriteLine($"[DEBUG] Email ingresado: {request.Email}");
                    System.Console.WriteLine($"[DEBUG] Email en minúsculas: {emailLower}");
                    
                    // Registrar intento fallido
                    await _unitOfWork.IntentosLogin.IncrementarIntentosAsync(request.Email, "", "");
                    await _unitOfWork.SaveChangesAsync();
                    System.Console.WriteLine($"[DEBUG] ✓ Intento fallido guardado en BD");

                    // Contar intentos fallidos consecutivos (últimas 3 horas)
                    var tresHorasAtras = DateTime.UtcNow.AddHours(-3);
                    var todoIntentosLogin = await _unitOfWork.IntentosLogin.GetAllAsync();
                    System.Console.WriteLine($"[DEBUG] Total registros en IntentosLogin: {todoIntentosLogin.Count()}");
                    
                    // Listar todos los intentos recientes para este email
                    var intentosEsteEmail = todoIntentosLogin
                        .Where(il => il.NombreUsuario.ToLower() == emailLower)
                        .OrderByDescending(il => il.FechaIntento)
                        .ToList();
                    
                    System.Console.WriteLine($"[DEBUG] Intentos TOTALES para {emailLower}: {intentosEsteEmail.Count()}");
                    foreach (var intento in intentosEsteEmail.Take(5))
                    {
                        System.Console.WriteLine($"  - {intento.FechaIntento} | Exitoso: {intento.Exitoso} | Email: {intento.NombreUsuario}");
                    }
                    
                    var intentosFallidosConsecutivos = todoIntentosLogin
                        .Where(il => il.NombreUsuario.ToLower() == emailLower
                            && !il.Exitoso 
                            && il.FechaIntento >= tresHorasAtras)
                        .OrderByDescending(il => il.FechaIntento)
                        .Count();

                    System.Console.WriteLine($"[DEBUG] Intentos fallidos en últimas 3 horas: {intentosFallidosConsecutivos}");
                    System.Console.WriteLine($"[DEBUG] ¿Es === 3? {intentosFallidosConsecutivos == 3}");
                    System.Console.WriteLine($"[DEBUG] ¿Es >= 3? {intentosFallidosConsecutivos >= 3}");

                    // Si alcanzó 3 intentos fallidos, desactivar el usuario
                    if (intentosFallidosConsecutivos >= 3)
                    {
                        System.Console.WriteLine($"\n╔════════════════════════════════════════╗");
                        System.Console.WriteLine($"║ [SECURITY] BLOQUEANDO USUARIO          ║");
                        System.Console.WriteLine($"║ Email: {emailLower}");
                        System.Console.WriteLine($"║ Razón: 3 intentos fallidos             ║");
                        System.Console.WriteLine($"╚════════════════════════════════════════╝\n");
                        
                        usuario.Activo = false;
                        await _unitOfWork.Usuarios.UpdateAsync(usuario);
                        System.Console.WriteLine($"[DEBUG] ✓ Usuario.Activo = false");

                        // Registrar la desactivación automática en EliminacionUsuario
                        var eliminacion = new EliminacionUsuario
                        {
                            UsuarioEliminadoId = usuario.Id,
                            CedulaUsuarioEliminado = usuario.NombreUsuario,
                            NombreUsuarioEliminado = usuario.Nombre,
                            EmailUsuarioEliminado = usuario.Email,
                            RolUsuarioEliminado = usuario.RolNombre,
                            AdministradorId = 0,
                            NombreAdministrador = "Sistema Automático",
                            FechaEliminacion = DateTime.UtcNow,
                            MotivoEliminacion = "Bloqueo automático por 3 intentos fallidos de login",
                            TipoEliminacion = "Desactivación",
                            IpAddress = ""
                        };

                        await _unitOfWork.EliminacionesUsuarios.AddAsync(eliminacion);
                        System.Console.WriteLine($"[DEBUG] ✓ Registro de desactivación creado");
                        
                        await _unitOfWork.SaveChangesAsync();
                        System.Console.WriteLine($"[DEBUG] ✓ CAMBIOS GUARDADOS EN BD");
                        System.Console.WriteLine($"[SECURITY] Usuario {emailLower} ha sido BLOQUEADO por 3 intentos fallidos\n");

                        response.Exitoso = false;
                        response.Mensaje = "Usuario Bloqueado Comuniquese con el Administrador del Sistema";
                        return response;
                    }

                    System.Console.WriteLine($"=====================================\n");
                    response.Exitoso = false;
                    response.Mensaje = "Email o contraseña incorrectos";
                    return response;
                }

                // Login exitoso: Limpiar intentos fallidos previos
                var emailLowerForClean = request.Email.ToLower();
                System.Console.WriteLine($"\n✅ [LOGIN EXITOSO] {emailLowerForClean}");
                var intentosFallidosPrevios = (await _unitOfWork.IntentosLogin.GetAllAsync())
                    .Where(il => il.NombreUsuario.ToLower() == emailLowerForClean && !il.Exitoso)
                    .ToList();
                
                foreach (var intento in intentosFallidosPrevios)
                {
                    await _unitOfWork.IntentosLogin.DeleteAsync(intento.Id);
                }
                System.Console.WriteLine($"[DEBUG] Limpiados {intentosFallidosPrevios.Count()} intentos fallidos previos");

                // Registrar el login exitoso
                await _unitOfWork.IntentosLogin.ReiniciarIntentosAsync(request.Email);

                // Actualizar última fecha de login
                usuario.FechaUltimoLogin = DateTime.UtcNow;
                await _unitOfWork.Usuarios.UpdateAsync(usuario);
                await _unitOfWork.SaveChangesAsync();

                // Generar JWT
                var token = GenerarToken(usuario);

                response.Exitoso = true;
                response.Token = token;
                response.UsuarioId = usuario.Id;
                response.NombreUsuario = usuario.NombreUsuario;
                response.NombreCompleto = usuario.Nombre;
                response.Correo = usuario.Email;
                response.Rol = usuario.RolNombre;
                response.Mensaje = "Login exitoso";
            }
            catch (Exception ex)
            {
                response.Exitoso = false;
                response.Mensaje = $"Error en el login: {ex.Message}";
            }

            return response;
        }

        private string GenerarToken(Domain.Entities.Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? ""),
                    new Claim(ClaimTypes.Surname, usuario.Apellido ?? ""),
                    new Claim("NombreUsuario", usuario.NombreUsuario ?? ""),
                    new Claim(ClaimTypes.Role, usuario.RolNombre ?? "Usuario")
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

