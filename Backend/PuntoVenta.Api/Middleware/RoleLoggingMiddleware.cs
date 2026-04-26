using System.Security.Claims;

namespace PuntoVenta.Api.Middleware
{
    /// <summary>
    /// Middleware para loguear los roles del usuario autenticado
    /// </summary>
    public class RoleLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleLoggingMiddleware> _logger;

        public RoleLoggingMiddleware(RequestDelegate next, ILogger<RoleLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si el usuario está autenticado, loguear sus roles
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var roles = context.User.FindAll(ClaimTypes.Role);
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                
                if (roles.Any())
                {
                    var roleList = string.Join(", ", roles.Select(r => r.Value));
                    _logger.LogInformation($"[ROLES] User: {username}, Roles: {roleList}");
                }
                else
                {
                    _logger.LogWarning($"[ROLES] User: {username}, NO ROLES FOUND!");
                }
            }

            await _next(context);
        }
    }
}
