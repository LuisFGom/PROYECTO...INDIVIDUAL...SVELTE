# 🚀 Guía de Inicio Rápido - VersionM

## Pre-requisitos Verificados ✅

- ✅ Base de datos PostgreSQL en ejecución (localhost:5432)
- ✅ Credenciales de BD actualizadas
- ✅ Todas las migraciones ejecutadas
- ✅ Tabla `eliminaciones_productos` creada
- ✅ VersionM Backend compilado sin errores
- ✅ 100,000+ registros de prueba disponibles

---

## Paso 1: Abrir Terminal PowerShell #1 (Backend)

```powershell
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Backend\PuntoVenta.Api"
dotnet run
```

**Resultado esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:56397
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:56398
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

⏱️ Tiempo: ~15 segundos

---

## Paso 2: Verificar Backend

Abre en tu navegador:
```
https://localhost:56397/
```

Deberías ver **Swagger UI** con todos los endpoints disponibles.

✅ Si lo ves, el Backend está funcionando correctamente.

---

## Paso 3: Abrir Terminal PowerShell #2 (Frontend)

```powershell
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Frontend"
npm run dev
```

**Resultado esperado:**
```
VITE v5.4.21  ready in 803 ms
Port 3000 is in use, trying another one...
Local:   http://localhost:3001/
```

Verás algo similar en tu consola.

⏱️ Tiempo: ~10 segundos

---

## Paso 4: Abrir la Aplicación

El Frontend debería abrirse automáticamente, pero si no:

```
http://localhost:3001/
```

Deberías ver una página de **Login**.

---

## Paso 5: Iniciar Sesión

Usa estas credenciales:

**Email:**
```
Sofia63@gmail.com
```

**Contraseña:**
```
Password123!
```

Haz clic en **"Iniciar Sesión"** o presiona **Enter**.

✅ Si la sesión inicia, todo está funcionando correctamente.

---

## Paso 6: Explorar la Aplicación

Una vez dentro, puedes acceder a:

### 1️⃣ Dashboard / Inicio
- Vista general del sistema
- Estadísticas rápidas

### 2️⃣ Clientes
- 100,000 clientes disponibles
- Búsqueda y filtrado
- Paginación avanzada (±10, ±100, ±1000 páginas)

### 3️⃣ Productos
- 100,000 productos disponibles
- Búsqueda y filtrado
- Paginación avanzada

### 4️⃣ Ventas/Facturas
- 10,000 facturas registradas
- Detalles de cada venta
- Historial completo

### 5️⃣ Usuarios (Si eres Admin)
- Gestión de usuarios
- Control de roles

---

## 🧪 Casos de Prueba Recomendados

### Prueba 1: Navegación Básica
```
1. Ir a Clientes
2. Verificar que se cargan 10 clientes por página
3. Hacer clic en "Siguiente" (>)
4. Verificar que la página cambia
```

### Prueba 2: Paginación Avanzada
```
1. Ir a Productos (50,000 páginas)
2. Haz clic en "+1000" varias veces
3. Observa cómo salta 1000 páginas por vez
4. Haz clic en "Última" para ir a la última página
```

### Prueba 3: Búsqueda
```
1. En Clientes, escribe el nombre de un cliente
2. Verifica que se filtren los resultados
3. Prueba con diferentes búsquedas
```

### Prueba 4: Crear Venta (si está disponible)
```
1. Navega a Ventas
2. Intenta crear una nueva factura
3. Selecciona cliente y productos
4. Verifica el cálculo de totales
```

---

## 🔧 Solución de Problemas

### Backend no inicia
```
❌ Error: "Address already in use"
✅ Solución: Cierra otros servicios en los puertos 56397/56398
   netstat -ano | findstr :56397
   taskkill /PID <PID> /F
```

### Frontend no compila
```
❌ Error: "npm command not found"
✅ Solución: Instala Node.js desde nodejs.org
```

### No puedo hacer login
```
❌ Error: "401 Unauthorized"
✅ Solución: 
   - Verifica que el Backend esté corriendo
   - Usa el email exacto: Sofia63@gmail.com
   - Verifica la contraseña: Password123!
```

### Base de datos desconectada
```
❌ Error: "Npgsql.NpgsqlException: Connection refused"
✅ Solución:
   - Verifica que PostgreSQL esté corriendo
   - Comprueba las credenciales:
     Host: localhost
     Port: 5432
     User: postgres
     Pass: 1593571177220011
```

---

## 📊 Monitoreo en Tiempo Real

### Backend Log
En la Terminal #1, verás todos los requests:
```
GET /api/clientes?page=1&pageSize=10
POST /api/usuarios/login
GET /swagger/v1/swagger.json
```

### Frontend Console
Abre DevTools (F12) en el navegador para ver logs y errores de JavaScript.

---

## 🎯 Confirmación de Éxito

✅ Has completado el setup cuando:

1. Backend responde en `https://localhost:56397/` (Swagger visible)
2. Frontend carga en `http://localhost:3001/` (Login visible)
3. Puedes hacer login con `Sofia63@gmail.com` / `Password123!`
4. Ves la lista de Clientes con paginación funcional
5. Puedes navegar entre páginas sin errores

---

## 📞 Información Adicional

**Documentación detallada:** `CONFIGURACION_BD.md`
**Resumen de cambios:** `RESUMEN_CONFIGURACION_VERSION_M.md`
**Readme general:** `README.md`

---

## 🏁 Listo para Usar

```
✅ Backend:  https://localhost:56397/
✅ Frontend: http://localhost:3001/
✅ BD:       PuntoVentaDb (localhost:5432)
✅ Usuario:  Sofia63@gmail.com
✅ Contraseña: Password123!
```

**¡Ahora estás listo para usar VersionM!** 🚀

---

*Configuración completada: 10 de diciembre de 2025*
