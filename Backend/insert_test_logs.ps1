[Reflection.Assembly]::LoadFile("C:\Users\ASUS\.nuget\packages\npgsql\10.0.2\lib\net6.0\Npgsql.dll")

$connString = "Host=localhost;Port=5432;Database=punto_venta;Username=postgres;Password=LuiS_2004ferG"
$conn = [Npgsql.NpgsqlConnection]::new($connString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
INSERT INTO "IntentosLogin" ("NombreUsuario", "Exitoso", "FechaIntento", "MensajeError", "IpAddress", "UserAgent")
VALUES 
  ('admin', true, NOW() - INTERVAL '2 days', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('admin', true, NOW() - INTERVAL '1 day', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('vendedor1', false, NOW() - INTERVAL '2 days', 'Contraseña incorrecta', '192.168.1.2', 'Chrome'),
  ('vendedor1', true, NOW() - INTERVAL '1 hour', NULL, '192.168.1.2', 'Chrome'),
  ('cajero1', true, NOW() - INTERVAL '30 minutes', NULL, '192.168.1.3', 'Firefox')
"@

try {
    $affected = $cmd.ExecuteNonQuery()
    Write-Host "Filas insertadas: $affected"
} catch {
    Write-Host "Error al insertar: $_"
}

$cmd.CommandText = 'SELECT COUNT(*) FROM "IntentosLogin"'
$count = $cmd.ExecuteScalar()
Write-Host "Total registros en IntentosLogin: $count"

$conn.Close()
