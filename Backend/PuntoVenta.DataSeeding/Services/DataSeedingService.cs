using Bogus;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure.Persistencia;
using System.Security.Cryptography;
using BCrypt.Net;

namespace PuntoVenta.DataSeeding.Services;

public class DataSeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly Faker _faker = new Faker("es_MX");
    private readonly Random _random = new Random();

    public DataSeedingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedDataAsync(int recordCount, int clientCount, int productCount)
    {
        try
        {
            // Limpiar datos primero
            await ClearAllDataAsync();
            
            // 1. Generar Roles
            Console.WriteLine("[1/7] Generando Roles...");
            var roles = await GenerateRolesAsync();
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {roles.Count} roles creados");

            // 2. Generar Usuarios
            Console.WriteLine("[2/7] Generando Usuarios...");
            var usuarios = GenerateUsuarios(recordCount / 100, roles);
            _context.Usuarios.AddRange(usuarios);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {usuarios.Count} usuarios creados");

            // 3. Generar Clientes
            Console.WriteLine("[3/7] Generando Clientes...");
            var clientes = GenerateClientes(clientCount);
            _context.Clientes.AddRange(clientes);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {clientes.Count} clientes creados");

            // 4. Generar Productos
            Console.WriteLine("[4/7] Generando Productos...");
            var productos = GenerateProductos(productCount);
            _context.Productos.AddRange(productos);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {productos.Count} productos creados");

            // 5. Generar Facturas (Ventas)
            Console.WriteLine("[5/7] Generando Facturas/Ventas...");
            var (facturas, detalles) = GenerateFacturasConDetalles(
                recordCount, 
                usuarios, 
                clientes, 
                productos
            );
            _context.Facturas.AddRange(facturas);
            _context.DetallesVenta.AddRange(detalles);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {facturas.Count} facturas y {detalles.Count} detalles creados");

            // 6. Generar Intentos de Login
            Console.WriteLine("[6/7] Generando Intentos de Login...");
            var intentos = GenerateIntentosLogin(recordCount / 50, usuarios);
            _context.IntentosLogin.AddRange(intentos);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {intentos.Count} intentos de login creados");

            // 7. Generar Error Logs
            Console.WriteLine("[7/7] Generando Error Logs...");
            var errorLogs = GenerateErrorLogs(recordCount / 100);
            _context.ErrorLogs.AddRange(errorLogs);
            await _context.SaveChangesAsync();
            Console.WriteLine($"     ✓ {errorLogs.Count} error logs creados");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error durante el seeding: {ex.Message}");
            throw;
        }
    }

    private async Task<List<Rol>> GenerateRolesAsync()
    {
        var roles = new List<Rol>
        {
            new Rol { Nombre = "Admin", Descripcion = "Administrador del sistema", Activo = true },
            new Rol { Nombre = "Vendedor", Descripcion = "Usuario vendedor", Activo = true },
            new Rol { Nombre = "Cajero", Descripcion = "Operario de caja", Activo = true },
            new Rol { Nombre = "Gerente", Descripcion = "Gerente de ventas", Activo = true },
            new Rol { Nombre = "Auditor", Descripcion = "Auditor del sistema", Activo = true }
        };

        _context.Roles.AddRange(roles);
        return await Task.FromResult(roles);
    }

    private List<Usuario> GenerateUsuarios(int count, List<Rol> roles)
    {
        var usuarios = new List<Usuario>();
        var usedEmails = new HashSet<string>();
        var usedUsernames = new HashSet<string>();
        
        // Crear usuario admin conocido para testing
        var adminRole = roles.First(r => r.Nombre == "Admin");
        var adminUser = new Usuario
        {
            NombreUsuario = "admin",
            Email = "admin@test.com",
            Nombre = "Administrador",
            Apellido = "Sistema",
            PasswordHash = HashPassword("Password123!"),
            Activo = true,
            RolId = adminRole.Id,
            RolNombre = adminRole.Nombre,
            FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };
        usuarios.Add(adminUser);
        usedEmails.Add("admin@test.com");
        usedUsernames.Add("admin");
        
        for (int i = 0; i < count; i++)
        {
            string email;
            string username;
            
            // Generar email único
            do
            {
                email = _faker.Internet.Email();
            } while (usedEmails.Contains(email));
            usedEmails.Add(email);
            
            // Generar username único
            do
            {
                username = _faker.Internet.UserName().Replace(".", "");
            } while (usedUsernames.Contains(username));
            usedUsernames.Add(username);

            var usuario = new Usuario
            {
                NombreUsuario = username,
                Email = email,
                Nombre = _faker.Person.FirstName,
                Apellido = _faker.Person.LastName,
                PasswordHash = HashPassword("Password123!"),
                Activo = _faker.Random.Bool(0.95f),
                RolId = roles[_faker.Random.Int(0, roles.Count - 1)].Id,
                RolNombre = roles[_faker.Random.Int(0, roles.Count - 1)].Nombre,
                FechaCreacion = DateTime.SpecifyKind(_faker.Date.PastDateOnly(50).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            };

            usuarios.Add(usuario);
        }

        return usuarios;
    }

    private List<Cliente> GenerateClientes(int count)
    {
        var clientes = new List<Cliente>();
        var usedDocuments = new HashSet<string>();
        var faker = new Faker("es_MX");

        for (int i = 0; i < count; i++)
        {
            string documento;
            do
            {
                documento = faker.Random.String2(10, "0123456789");
            } while (usedDocuments.Contains(documento));
            
            usedDocuments.Add(documento);

            var cliente = new Cliente
            {
                Nombre = faker.Person.FirstName,
                Apellido = faker.Person.LastName,
                Documento = documento,
                Direccion = faker.Address.FullAddress(),
                Telefono = faker.Phone.PhoneNumber(),
                Email = faker.Internet.Email(),
                Activo = faker.Random.Bool(0.98f),
                FechaCreacion = DateTime.SpecifyKind(faker.Date.PastDateOnly(30).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            };
            clientes.Add(cliente);
        }

        return clientes;
    }

    private List<Product> GenerateProductos(int count)
    {
        var faker = new Faker<Product>("es_MX")
            .RuleFor(p => p.Nombre, f => f.Commerce.ProductName())
            .RuleFor(p => p.Codigo, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(p => p.Descripcion, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Precio, f => decimal.Parse(f.Commerce.Price(10, 5000, 2)))
            .RuleFor(p => p.PrecioCompra, (f, p) => p.Precio * (0.5m + (decimal)f.Random.Double(0, 0.3)))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 1000))
            .RuleFor(p => p.StockMinimo, f => f.Random.Int(5, 50))
            .RuleFor(p => p.Activo, f => f.Random.Bool(0.98f))
            .RuleFor(p => p.FechaCreacion, f => DateTime.SpecifyKind(f.Date.PastDateOnly(60).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));

        return faker.Generate(count);
    }

    private (List<Factura>, List<DetalleVenta>) GenerateFacturasConDetalles(
        int count, 
        List<Usuario> usuarios, 
        List<Cliente> clientes, 
        List<Product> productos)
    {
        var facturas = new List<Factura>();
        var detalles = new List<DetalleVenta>();
        var numeroFactura = 1000001;

        for (int i = 0; i < count; i++)
        {
            var usuarioSeleccionado = usuarios[_random.Next(usuarios.Count)];
            var clienteSeleccionado = _random.Next(100) > 20 ? clientes[_random.Next(clientes.Count)] : null;
            
            var factura = new Factura
            {
                NumeroFactura = (numeroFactura++).ToString(),
                FechaVenta = DateTime.SpecifyKind(_faker.Date.PastDateOnly(60).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                UsuarioId = usuarioSeleccionado.Id,
                UsuarioNombre = usuarioSeleccionado.Nombre,
                ClienteId = clienteSeleccionado?.Id,
                ClienteNombre = clienteSeleccionado?.Nombre ?? string.Empty,
                ClienteDocumento = clienteSeleccionado?.Documento,
                Subtotal = 0,
                PorcentajeIVA = 12m,
                TotalImpuesto = 0,
                TotalVenta = 0,
                Estado = "Completada",
                Detalles = new List<DetalleVenta>()
            };

            // Generar 1-10 detalles por factura
            var cantidadDetalles = _random.Next(1, 11);
            decimal subtotal = 0;

            for (int j = 0; j < cantidadDetalles; j++)
            {
                var producto = productos[_random.Next(productos.Count)];
                var cantidad = _random.Next(1, 20);
                var precioUnitario = producto.Precio;
                var lineTotal = cantidad * precioUnitario;

                var detalle = new DetalleVenta
                {
                    FacturaId = factura.Id,
                    ProductoId = producto.Id,
                    ProductoNombre = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Descuento = 0,
                    Total = lineTotal
                };

                factura.Detalles.Add(detalle);
                detalles.Add(detalle);
                subtotal += lineTotal;
            }

            factura.Subtotal = subtotal;
            factura.TotalImpuesto = subtotal * (factura.PorcentajeIVA / 100);
            factura.TotalVenta = subtotal + factura.TotalImpuesto;

            facturas.Add(factura);
        }

        return (facturas, detalles);
    }

    private List<IntentosLogin> GenerateIntentosLogin(int count, List<Usuario> usuarios)
    {
        var intentos = new List<IntentosLogin>();
        
        for (int i = 0; i < count; i++)
        {
            var usuario = usuarios[_random.Next(usuarios.Count)];
            var exitoso = _faker.Random.Bool(0.85f);
            
            var intento = new IntentosLogin
            {
                NombreUsuario = usuario.NombreUsuario,
                Exitoso = exitoso,
                IpAddress = _faker.Internet.IpAddress().ToString(),
                UserAgent = _faker.Internet.UserAgent(),
                FechaIntento = DateTime.SpecifyKind(_faker.Date.PastDateOnly(30).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                MensajeError = exitoso ? null : _faker.Lorem.Sentence()
            };

            intentos.Add(intento);
        }

        return intentos;
    }

    private List<ErrorLog> GenerateErrorLogs(int count)
    {
        var errorLogs = new List<ErrorLog>();
        var tiposError = new[] { "NullReferenceException", "InvalidOperationException", "ArgumentException", "TimeoutException" };
        var niveles = new[] { "Error", "Warning", "Info" };
        
        for (int i = 0; i < count; i++)
        {
            var errorLog = new ErrorLog
            {
                TipoError = _faker.PickRandom(tiposError),
                Mensaje = _faker.Lorem.Sentence(),
                StackTrace = _faker.Lorem.Paragraphs(2),
                Origen = _faker.PickRandom(new[] { "UsuariosController", "ProductController", "VentasController", "ClientesController" }),
                NumeroLinea = _faker.Random.Int(1, 500),
                UsuarioId = _faker.Random.String2(10),
                Nivel = _faker.PickRandom(niveles),
                Fecha = DateTime.SpecifyKind(_faker.Date.PastDateOnly(60).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                Revisado = _faker.Random.Bool(0.3f),
                Notas = _faker.Random.Bool(0.5f) ? _faker.Lorem.Sentence() : null
            };

            errorLogs.Add(errorLog);
        }

        return errorLogs;
    }

    public async Task ClearAllDataAsync()
    {
        // Eliminar en orden inverso de dependencias
        try
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"detalles_venta\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"facturas\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"intentos_login\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"error_logs\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"eliminaciones_usuarios\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"usuarios\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"clientes\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"productos\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"roles\" CASCADE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia al limpiar datos: {ex.Message}");
        }
    }

    public async Task DisplayDataSummaryAsync()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     RESUMEN DE DATOS GENERADOS         ║");
        Console.WriteLine("╠════════════════════════════════════════╣");

        var rolesCount = await _context.Roles.CountAsync();
        Console.WriteLine($"║ Roles              : {rolesCount,31} ║");

        var usuariosCount = await _context.Usuarios.CountAsync();
        Console.WriteLine($"║ Usuarios           : {usuariosCount,31} ║");

        var clientesCount = await _context.Clientes.CountAsync();
        Console.WriteLine($"║ Clientes           : {clientesCount,31} ║");

        var productosCount = await _context.Productos.CountAsync();
        Console.WriteLine($"║ Productos          : {productosCount,31} ║");

        var facturasCount = await _context.Facturas.CountAsync();
        Console.WriteLine($"║ Facturas           : {facturasCount,31} ║");

        var detallesCount = await _context.DetallesVenta.CountAsync();
        Console.WriteLine($"║ Detalles de Venta  : {detallesCount,31} ║");

        var intentosCount = await _context.IntentosLogin.CountAsync();
        Console.WriteLine($"║ Intentos de Login  : {intentosCount,31} ║");

        var errorsCount = await _context.ErrorLogs.CountAsync();
        Console.WriteLine($"║ Error Logs         : {errorsCount,31} ║");

        Console.WriteLine("╚════════════════════════════════════════╝\n");
    }

    private static string HashPassword(string password)
    {
        // Usar BCrypt.EnhancedHashPassword igual que el sistema de autenticación
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, hashType: HashType.SHA384);
    }
}
