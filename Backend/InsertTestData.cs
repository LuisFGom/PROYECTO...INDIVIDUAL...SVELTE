using System;
using Npgsql;
using NpgsqlTypes;

class Program
{
    static void Main()
    {
        string connString = "Host=localhost;Port=5432;Database=punto_venta;Username=postgres;Password=LuiS_2004ferG";
        
        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO ""IntentosLogin"" (""NombreUsuario"", ""Exitoso"", ""FechaIntento"", ""MensajeError"", ""IpAddress"", ""UserAgent"")
VALUES 
  ('admin', true, NOW() - INTERVAL '2 days', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('admin', true, NOW() - INTERVAL '1 day', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('vendedor1', false, NOW() - INTERVAL '2 days', 'Contraseña incorrecta', '192.168.1.2', 'Chrome'),
  ('vendedor1', true, NOW() - INTERVAL '1 hour', NULL, '192.168.1.2', 'Chrome'),
  ('cajero1', true, NOW() - INTERVAL '30 minutes', NULL, '192.168.1.3', 'Firefox')
";
                
                try
                {
                    int affected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Filas insertadas: {affected}");
                }
                catch (NpgsqlException ex)
                {
                    Console.WriteLine($"Error al insertar: {ex.Message}");
                }
            }
            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(*) FROM ""IntentosLogin""";
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"Total registros en IntentosLogin: {count}");
            }
        }
    }
}
