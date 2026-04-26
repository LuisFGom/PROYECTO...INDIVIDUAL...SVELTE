[Reflection.Assembly]::LoadFile("C:\Users\ASUS\.nuget\packages\npgsql\10.0.2\lib\net6.0\Npgsql.dll")

$connString = "Host=localhost;Port=5432;Database=punto_venta;Username=postgres;Password=LuiS_2004ferG"
$conn = [Npgsql.NpgsqlConnection]::new($connString)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM IntentosLogin"
$count = $cmd.ExecuteScalar()

Write-Host "Total registros en IntentosLogin: $count"

$cmd.CommandText = "SELECT * FROM IntentosLogin LIMIT 5"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "ID: $($reader[0]), Usuario: $($reader[1]), Exitoso: $($reader[2]), Fecha: $($reader[4])"
}

$reader.Close()
$conn.Close()
