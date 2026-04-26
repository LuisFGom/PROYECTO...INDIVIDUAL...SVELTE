using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistencia;
using PuntoVenta.Application.Services;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Options;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using PuntoVenta.Api.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar Servicios de Infraestructura (DB, Identity, Repositorios)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Configurar y registrar SmtpSettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionKey));

// 3. Registrar servicios de aplicación
builder.Services.AddScoped<IEmailValidationService, EmailValidationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 3. Registrar MediatR - Escanear el ensamblado de Application donde están los handlers
builder.Services.AddMediatR(typeof(PuntoVenta.Application.Features.Usuarios.Commands.CreateUsuarioCommand).Assembly);

// Lectura de la clave secreta desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "EstaEsUnaClaveSuperSecretaDeAlMenos32CaracteresParaFirmarJWT");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// Registrar el documento Swagger "v1" con autenticación JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PuntoVenta API",
        Version = "v1",
        Description = "API de ejemplo para PuntoVenta"
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT en el formato: Bearer {tu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PuntoVenta.Infrastructure.Persistencia.ApplicationDbContext>();
    try
    {
        Console.WriteLine("[STARTUP] Aplicando migraciones de base de datos...");
        dbContext.Database.Migrate();
        Console.WriteLine("[STARTUP] Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP ERROR] Error al aplicar migraciones: {ex.Message}");
        // Intentar crear la base de datos si no existe
        try
        {
            Console.WriteLine("[STARTUP] Intentando crear base de datos...");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("[STARTUP] Base de datos creada/verificada.");
        }
        catch (Exception ex2)
        {
            Console.WriteLine($"[STARTUP ERROR] Error al crear base de datos: {ex2.Message}");
        }
    }
}

// Habilitar Swagger en todos los entornos (para producción también)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = string.Empty; // Swagger UI en la raíz "/"
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PuntoVenta API v1");
});

// En desarrollo, no redirigir HTTP a HTTPS para evitar problemas con CORS preflight
if (!app.Environment.IsDevelopment())
{
    // Deshabilitado para Railway ya que maneja HTTPS a nivel de proxy
    // app.UseHttpsRedirection();
}

app.UseExceptionHandlingMiddleware();

// Middleware para loguear roles
app.UseMiddleware<PuntoVenta.Api.Middleware.RoleLoggingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirección opcional: si alguien abre "/", redirigir a "/swagger"
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
