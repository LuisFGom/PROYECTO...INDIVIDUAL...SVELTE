using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistencia;
using PuntoVenta.DataSeeding.Services;

Console.WriteLine("=== GENERADOR DE DATOS DE PRUEBA - PUNTO DE VENTA ===\n");

try
{
    // Configurar servicios
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var services = new ServiceCollection();
    services.AddSingleton(configuration);
    services.AddInfrastructureServices(configuration);
    services.AddScoped<DataSeedingService>();

    var serviceProvider = services.BuildServiceProvider();

    // Obtener configuración de cantidad de registros
    Console.Write("Cantidad de registros base para facturas (default 10000): ");
    var input = Console.ReadLine();
    var recordCount = int.TryParse(input, out var count) ? count : 10000;

    Console.Write("Cantidad de clientes (default 100000): ");
    input = Console.ReadLine();
    var clientCount = int.TryParse(input, out var cCount) ? cCount : 100000;

    Console.Write("Cantidad de productos (default 100000): ");
    input = Console.ReadLine();
    var productCount = int.TryParse(input, out var pCount) ? pCount : 100000;

    Console.WriteLine($"\nConfiguración:");
    Console.WriteLine($"  - Facturas base: {recordCount}");
    Console.WriteLine($"  - Clientes: {clientCount}");
    Console.WriteLine($"  - Productos: {productCount}\n");

    // Ejecutar seeding
    using (var scope = serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();

        // Verificar conexión
        Console.WriteLine("Verificando conexión a la base de datos...");
        if (await dbContext.Database.CanConnectAsync())
        {
            Console.WriteLine("✓ Conexión exitosa!\n");

            // Ejecutar migraciones
            Console.WriteLine("Aplicando migraciones...");
            try
            {
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("✓ Migraciones completadas!\n");
            }
            catch (Exception migEx)
            {
                Console.WriteLine($"⚠ Migraciones ya aplicadas o error: {migEx.Message}\n");
            }

            // Limpiar datos existentes (opcional)
            Console.Write("¿Deseas limpiar los datos existentes? (s/n): ");
            if (Console.ReadLine()?.ToLower() == "s")
            {
                Console.WriteLine("\nLimpiando datos existentes...");
                await seedingService.ClearAllDataAsync();
                Console.WriteLine("✓ Datos eliminados!\n");
            }

            // Generar datos
            Console.WriteLine($"Generando registros de prueba...\n");
            var startTime = DateTime.Now;

            await seedingService.SeedDataAsync(recordCount, clientCount, productCount);

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"\n✓ ¡Seeding completado en {duration.TotalSeconds:F2} segundos!\n");

            // Mostrar resumen
            await seedingService.DisplayDataSummaryAsync();
        }
        else
        {
            Console.WriteLine("✗ Error: No se pudo conectar a la base de datos.");
            Console.WriteLine("Verifica la cadena de conexión en appsettings.json");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error durante el seeding: {ex.Message}");
    Console.WriteLine($"\nDetalles:\n{ex}");
    Environment.Exit(1);
}
