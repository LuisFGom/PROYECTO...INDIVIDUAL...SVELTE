using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistencia;
using BCrypt.Net;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton(configuration);
services.AddInfrastructureServices(configuration);

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("Limpiando datos existentes...");
    
    // Limpiar datos en orden inverso de dependencias
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"detalles_venta\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"facturas\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"intentos_login\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"error_logs\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"eliminaciones_usuarios\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"eliminaciones_productos\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"usuarios\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"clientes\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"productos\" CASCADE");
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"roles\" CASCADE");
    
    Console.WriteLine("✓ Datos eliminados\n");

    // Crear roles
    Console.WriteLine("Creando roles...");
    var roles = new List<Rol>
    {
        new Rol { Nombre = "Admin", Descripcion = "Administrador del sistema", Activo = true, FechaCreacion = DateTime.UtcNow, Permisos = new[] { "manage_users", "manage_products" } },
        new Rol { Nombre = "Vendedor", Descripcion = "Usuario vendedor", Activo = true, FechaCreacion = DateTime.UtcNow, Permisos = new[] { "create_sales", "view_products" } },
        new Rol { Nombre = "Cajero", Descripcion = "Operario de caja", Activo = true, FechaCreacion = DateTime.UtcNow, Permisos = new[] { "process_sales" } },
        new Rol { Nombre = "Gerente", Descripcion = "Gerente de ventas", Activo = true, FechaCreacion = DateTime.UtcNow, Permisos = new[] { "manage_sales" } },
        new Rol { Nombre = "Auditor", Descripcion = "Auditor del sistema", Activo = true, FechaCreacion = DateTime.UtcNow, Permisos = new[] { "view_logs" } }
    };
    context.Roles.AddRange(roles);
    await context.SaveChangesAsync();
    Console.WriteLine($"✓ {roles.Count} roles creados\n");

    // Crear usuarios
    Console.WriteLine("Creando usuarios de prueba...");
    var adminRole = roles.First(r => r.Nombre == "Admin");
    
    var usuarios = new List<Usuario>
    {
        new Usuario
        {
            NombreUsuario = "admin",
            Email = "admin@test.com",
            Nombre = "Administrador",
            Apellido = "Sistema",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!", hashType: HashType.SHA384),
            Activo = true,
            RolId = adminRole.Id,
            RolNombre = adminRole.Nombre,
            FechaCreacion = DateTime.UtcNow
        },
        new Usuario
        {
            NombreUsuario = "juan_perez",
            Email = "juan.perez@test.com",
            Nombre = "Juan",
            Apellido = "Pérez García",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!", hashType: HashType.SHA384),
            Activo = true,
            RolId = roles.First(r => r.Nombre == "Vendedor").Id,
            RolNombre = "Vendedor",
            FechaCreacion = DateTime.UtcNow
        },
        new Usuario
        {
            NombreUsuario = "maria_lopez",
            Email = "maria.lopez@test.com",
            Nombre = "María",
            Apellido = "López Martínez",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!", hashType: HashType.SHA384),
            Activo = true,
            RolId = roles.First(r => r.Nombre == "Cajero").Id,
            RolNombre = "Cajero",
            FechaCreacion = DateTime.UtcNow
        }
    };
    
    context.Usuarios.AddRange(usuarios);
    await context.SaveChangesAsync();
    Console.WriteLine($"✓ {usuarios.Count} usuarios creados\n");

    // Crear clientes de ejemplo
    Console.WriteLine("Creando clientes...");
    var clientes = new List<Cliente>();
    for (int i = 1; i <= 5; i++)
    {
        clientes.Add(new Cliente
        {
            Nombre = $"Cliente Ejemplo {i}",
            Documento = $"100000000{i}",
            Direccion = $"Calle Principal {i}, Ciudad",
            Telefono = $"555-000{i:D2}",
            Email = $"cliente{i}@example.com",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        });
    }
    context.Clientes.AddRange(clientes);
    await context.SaveChangesAsync();
    Console.WriteLine($"✓ {clientes.Count} clientes creados\n");

    // Crear productos de ejemplo
    Console.WriteLine("Creando productos...");
    var productos = new List<Producto>();
    var productNames = new[] { "Laptop", "Mouse", "Teclado", "Monitor", "Auriculares", "Webcam", "Disco Duro", "SSD" };
    for (int i = 0; i < productNames.Length; i++)
    {
        productos.Add(new Producto
        {
            Nombre = productNames[i],
            Codigo = $"PROD{i + 1:D3}",
            Descripcion = $"Producto de ejemplo: {productNames[i]}",
            Precio = 100 + (i * 50),
            PrecioCompra = 70 + (i * 30),
            Stock = 100,
            StockMinimo = 10,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        });
    }
    context.Productos.AddRange(productos);
    await context.SaveChangesAsync();
    Console.WriteLine($"✓ {productos.Count} productos creados\n");

    Console.WriteLine("╔════════════════════════════════════╗");
    Console.WriteLine("║  ✓ INICIALIZACIÓN COMPLETADA       ║");
    Console.WriteLine("╠════════════════════════════════════╣");
    Console.WriteLine("║  Usuario: admin@test.com           ║");
    Console.WriteLine("║  Contraseña: Password123!         ║");
    Console.WriteLine("║  Rol: Admin                        ║");
    Console.WriteLine("╚════════════════════════════════════╝\n");
}
