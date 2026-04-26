using PuntoVenta.Infrastructure.Data;
using PuntoVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=punto_venta;Username=postgres;Password=LuiS_2004ferG");

using (var context = new ApplicationDbContext(optionsBuilder.Options))
{
    // Insert test login attempts
    var testAttempts = new List<IntentosLogin>
    {
        new IntentosLogin 
        { 
            NombreUsuario = "admin", 
            Exitoso = true, 
            FechaIntento = DateTime.UtcNow.AddDays(-2), 
            IpAddress = "192.168.1.1", 
            UserAgent = "Mozilla/5.0" 
        },
        new IntentosLogin 
        { 
            NombreUsuario = "admin", 
            Exitoso = true, 
            FechaIntento = DateTime.UtcNow.AddDays(-1), 
            IpAddress = "192.168.1.1", 
            UserAgent = "Mozilla/5.0" 
        },
        new IntentosLogin 
        { 
            NombreUsuario = "vendedor1", 
            Exitoso = false, 
            FechaIntento = DateTime.UtcNow.AddDays(-2), 
            MensajeError = "Contraseña incorrecta", 
            IpAddress = "192.168.1.2", 
            UserAgent = "Chrome" 
        },
        new IntentosLogin 
        { 
            NombreUsuario = "vendedor1", 
            Exitoso = true, 
            FechaIntento = DateTime.UtcNow.AddHours(-1), 
            IpAddress = "192.168.1.2", 
            UserAgent = "Chrome" 
        },
        new IntentosLogin 
        { 
            NombreUsuario = "cajero1", 
            Exitoso = true, 
            FechaIntento = DateTime.UtcNow.AddMinutes(-30), 
            IpAddress = "192.168.1.3", 
            UserAgent = "Firefox" 
        }
    };

    context.IntentosLogin.AddRange(testAttempts);
    var affected = context.SaveChanges();
    Console.WriteLine($"✅ {affected} registros insertados en IntentosLogin");
    
    var count = context.IntentosLogin.Count();
    Console.WriteLine($"📊 Total registros en IntentosLogin: {count}");
}
