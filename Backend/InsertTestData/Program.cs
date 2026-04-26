using Npgsql;

string connectionString = "Host=localhost;Port=5432;Database=punto_venta;Username=postgres;Password=LuiS_2004ferG";

try
{
    using (var connection = new NpgsqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("✅ Conectado a PostgreSQL\n");

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
INSERT INTO intentos_login (""NombreUsuario"", ""Exitoso"", ""FechaIntento"", ""MensajeError"", ""IpAddress"", ""UserAgent"")
VALUES 
  ('admin', true, NOW() - INTERVAL '2 days', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('admin', true, NOW() - INTERVAL '1 day', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('vendedor1', false, NOW() - INTERVAL '2 days', 'Contraseña incorrecta', '192.168.1.2', 'Chrome'),
  ('vendedor1', true, NOW() - INTERVAL '1 hour', NULL, '192.168.1.2', 'Chrome'),
  ('cajero1', true, NOW() - INTERVAL '30 minutes', NULL, '192.168.1.3', 'Firefox')
";

            try
            {
                int affected = command.ExecuteNonQuery();
                Console.WriteLine($"✅ {affected} registros insertados correctamente");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"⚠️ Error al insertar: {ex.Message}");
                if (ex.Message.Contains("duplicate") || ex.Message.Contains("ya existe"))
                {
                    Console.WriteLine("ℹ️ Los registros ya existen en la base de datos");
                }
            }
        }

        // Verify count
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM intentos_login";
            var count = command.ExecuteScalar();
            Console.WriteLine($"📊 Total de registros en intentos_login: {count}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
