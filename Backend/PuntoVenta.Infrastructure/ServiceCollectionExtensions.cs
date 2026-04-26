using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Infrastructure.Persistencia;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Repositories;
using PuntoVenta.Infrastructure.Services;

namespace PuntoVenta.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Obtener connection string - Railway usa DATABASE_URL
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Railway provee URL en formato: postgresql://user:pass@host:port/db
                // Convertir a formato Npgsql si es necesario
                if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
                }
            }
            else
            {
                // Usar connection string de appsettings (desarrollo local)
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }
            
            // Register PostgreSQL DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IIntentosLoginRepository, IntentosLoginRepository>();
            
            // Register Services
            services.AddScoped<ILoggingService, LoggingService>();
            
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

