# Generador de Datos de Prueba - Punto de Venta

## Descripción

Este proyecto genera 10,000 (o más) registros de datos para pruebas de rendimiento en la base de datos PostgreSQL del sistema de punto de venta.

## Características

- ✅ Generación de datos realistas usando **Bogus**
- ✅ Soporte para todas las entidades principales
- ✅ Opción para limpiar datos existentes antes de generar nuevos
- ✅ Interfaz interactiva fácil de usar
- ✅ Resumen detallado de registros generados
- ✅ Medición de tiempo de ejecución

## Qué genera

El script genera automáticamente:

| Entidad | Cantidad | Descripción |
|---------|----------|-------------|
| **Roles** | 5 | Admin, Vendedor, Cajero, Gerente, Auditor |
| **Usuarios** | RecordCount / 100 | Usuarios con credenciales de prueba |
| **Clientes** | RecordCount / 50 | Clientes con información ficticia |
| **Productos** | RecordCount / 20 | Productos con precios y stock |
| **Facturas** | RecordCount | Ventas/Facturas con detalles asociados |
| **Detalles de Venta** | RecordCount × 5 aprox. | Líneas de detalle en facturas |
| **Intentos de Login** | RecordCount / 50 | Registros de intentos de acceso |
| **Error Logs** | RecordCount / 100 | Logs de errores del sistema |

**Total: ~15,000+ registros para 10,000 como cantidad base**

## Requisitos Previos

1. **.NET 8.0** SDK instalado
2. **PostgreSQL** ejecutándose localmente
3. **Credenciales correctas** en `appsettings.json`
4. **Migraciones de base de datos** aplicadas

## Instalación

### Opción 1: Usando el Script PowerShell (Recomendado)

```powershell
# Navega a la carpeta Backend
cd C:\Users\Usuario\Desktop\Bryan_el_vitamina\Backend

# Ejecuta el script (requiere cantidad opcional)
.\GenerateTestData.ps1
```

### Opción 2: Ejecución Manual

```bash
# Navega al proyecto DataSeeding
cd C:\Users\Usuario\Desktop\Bryan_el_vitamina\Backend\PuntoVenta.DataSeeding

# Ejecuta la aplicación
dotnet run

# O compilar y ejecutar en Release
dotnet run --configuration Release
```

## Uso

### Ejecución Interactiva

1. Ejecuta el script o la aplicación
2. Se te pedirá que ingreses la cantidad de registros (default: 10,000)
3. Se verificará la conexión a la base de datos
4. Se aplicarán las migraciones si es necesario
5. Se preguntará si deseas limpiar los datos existentes
6. Se generarán los datos automáticamente
7. Se mostrará un resumen al finalizar

### Ejemplo de Ejecución

```
=== GENERADOR DE DATOS DE PRUEBA - PUNTO DE VENTA ===

Ingresa la cantidad de registros a generar (default 10000): 10000
Cantidad de registros a generar: 10000

Verificando conexión a la base de datos...
✓ Conexión exitosa!

Aplicando migraciones...
✓ Migraciones completadas!

¿Deseas limpiar los datos existentes? (s/n): s

Limpiando datos existentes...
✓ Datos eliminados!

Generando 10000 registros de prueba...

[1/7] Generando Roles...
     ✓ 5 roles creados
[2/7] Generando Usuarios...
     ✓ 100 usuarios creados
[3/7] Generando Clientes...
     ✓ 200 clientes creados
...

✓ ¡Seeding completado en 25.34 segundos!

╔════════════════════════════════════════╗
║     RESUMEN DE DATOS GENERADOS         ║
╠════════════════════════════════════════╣
║ Roles              :                  5 ║
║ Usuarios           :                100 ║
║ Clientes           :                200 ║
║ Productos          :                500 ║
║ Facturas           :              10000 ║
║ Detalles de Venta  :              50000 ║
║ Intentos de Login  :                200 ║
║ Error Logs         :                100 ║
╚════════════════════════════════════════╝
```

## Configuración

### appsettings.json

El archivo `PuntoVenta.DataSeeding\appsettings.json` contiene la configuración de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=PuntoVentaDb;Username=postgres;Password=1593571177220011"
  }
}
```

**Importante:** Actualiza las credenciales si tu PostgreSQL tiene diferentes valores.

## Datos Generados

### Usuarios

- **NombreUsuario:** Generados aleatoriamente
- **Email:** Emails únicos y válidos
- **Nombre:** Nombres completos en español
- **PasswordHash:** `Password123!` (hasheada)
- **RolId:** Asignado aleatoriamente de los roles existentes
- **FechaCreacion:** Últimos 50 días

### Productos

- **Nombre:** Nombres de productos reales
- **Código:** Códigos alfanuméricos únicos de 10 caracteres
- **Descripción:** Descripciones automáticas
- **Precio:** 10 - 5,000 (moneda local)
- **PrecioCompra:** 50% del precio de venta (±30%)
- **Stock:** 0 - 1,000 unidades
- **FechaCreacion:** Últimos 60 días

### Clientes

- **Nombre:** Nombres completos en español
- **Documento:** 10 dígitos aleatorios
- **Dirección:** Direcciones completas ficticias
- **Teléfono:** Números telefónicos válidos
- **Email:** Emails únicos
- **FechaCreacion:** Últimos 30 días

### Facturas/Ventas

- **NumeroFactura:** Secuencial comenzando desde 1000001
- **FechaVenta:** Últimos 60 días
- **UsuarioId:** Usuarios aleatorios del sistema
- **ClienteId:** 80% tienen cliente asignado
- **Detalles:** 1-10 líneas de detalle por factura
- **IVA:** 19% (configurado)
- **Estado:** "Completada"

### Intentos de Login

- **UsuarioId:** Usuarios aleatorios
- **Exitoso:** 85% de éxito
- **IpAddress:** Direcciones IPv4 válidas
- **FechaIntento:** Últimos 30 días

### Error Logs

- **Nivel:** Info, Warning, Error
- **Controlador:** UsuariosController, ProductController, VentasController, ClientesController
- **Método:** Get, Post, Put, Delete
- **FechaError:** Últimos 60 días

## Notas sobre el Rendimiento

- El proceso completo para 10,000 registros base toma aproximadamente **20-30 segundos**
- Se generan más de 50,000 registros en total (detalles de venta)
- Usa transacciones de base de datos para mejor rendimiento
- Los índices se mantienen durante la inserción para máxima eficiencia

## Troubleshooting

### Error de Conexión

```
Error: No se pudo conectar a la base de datos.
Verifica la cadena de conexión en appsettings.json
```

**Solución:**
1. Verifica que PostgreSQL está ejecutándose
2. Confirma el host, puerto, usuario y contraseña
3. Asegúrate de que la base de datos `PuntoVentaDb` existe

### Error de Migraciones

```
Build failed. Unable to create services...
```

**Solución:**
1. Verifica que estás en la rama correcta
2. Ejecuta las migraciones manualmente primero:
   ```bash
   cd PuntoVenta.Api
   dotnet ef database update --startup-project . --project ..\PuntoVenta.Infrastructure\PuntoVenta.Infrastructure.csproj
   ```

### Datos Duplicados

Si tienes datos previos y quieres reemplazarlos:
1. Ejecuta el script
2. Responde `s` cuando pregunte si deseas limpiar datos existentes
3. Los datos antiguos se eliminarán y se generarán nuevos

## Limpiar Datos Después de Pruebas

Para eliminar todos los datos de prueba:

```csharp
// En el código de la aplicación
using (var scope = serviceProvider.CreateScope())
{
    var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    await seedingService.ClearAllDataAsync();
}
```

O ejecuta SQL directamente:

```sql
TRUNCATE TABLE detalles_venta CASCADE;
TRUNCATE TABLE facturas CASCADE;
TRUNCATE TABLE intentos_login CASCADE;
TRUNCATE TABLE error_logs CASCADE;
TRUNCATE TABLE eliminaciones_usuarios CASCADE;
TRUNCATE TABLE usuarios CASCADE;
TRUNCATE TABLE clientes CASCADE;
TRUNCATE TABLE productos CASCADE;
TRUNCATE TABLE roles CASCADE;
```

## Licencia

Este proyecto es parte del sistema de Punto de Venta.

## Soporte

Para reportar problemas o sugerencias, contacta al equipo de desarrollo.
