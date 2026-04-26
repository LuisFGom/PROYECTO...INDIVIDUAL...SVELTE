# VersionM - Sistema de Punto de Venta Avanzado

## 🎯 Estado de Configuración

✅ **La versión avanzada está completamente configurada y lista para ejecutarse**

---

## ⚡ Quick Start

### Opción 1: Ejecutar ambos servidores en paralelo

**Terminal 1 - Backend:**
```bash
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Backend\PuntoVenta.Api"
dotnet run
```
Resultado esperado: Escucha en `https://localhost:56397` y `http://localhost:56398`

**Terminal 2 - Frontend:**
```bash
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Frontend"
npm run dev
```
Resultado esperado: Se abre en `http://localhost:3001`

### Opción 2: Usando PowerShell Scripts
```bash
# Backend
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Backend\PuntoVenta.Api"; dotnet run &

# Frontend
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Frontend"; npm run dev
```

---

## 🔐 Credenciales de Acceso

**Servidor de BD:** localhost:5432
- Usuario: `postgres`
- Contraseña: `1593571177220011`
- Base de datos: `PuntoVentaDb`

**Usuarios de prueba en la aplicación:**
- Email: `Sofia63@gmail.com`
- Contraseña: `Password123!`

Hay más de 100 usuarios disponibles en la BD.

---

## 🔄 Sincronización Realizada

### ✅ Base de Datos
- Credenciales actualizadas a las correctas
- Migración `AddEliminacionesProductos` ejecutada
- Tabla `eliminaciones_productos` creada y funcional

### ✅ Backend
- `appsettings.json` sincronizado
- JSON serialization configurado (soporta camelCase)
- Entidad `EliminacionProducto` agregada
- Compilación: 0 errores, 0 advertencias

### ✅ Datos Compartidos
- ~100,000 clientes
- ~100,000 productos
- ~10,000 facturas con detalles
- Sistema de auditoría completo

---

## 📁 Estructura

```
VersionM/
├── Backend/
│   ├── PuntoVenta.Api/           # API Backend
│   ├── PuntoVenta.Application/   # Lógica de negocio
│   ├── PuntoVenta.Domain/        # Entidades
│   └── PuntoVenta.Infrastructure/ # EF Core, Repositorios
└── Frontend/
    ├── src/
    │   ├── components/           # Componentes Vue
    │   ├── views/               # Páginas
    │   ├── services/            # APIs
    │   └── stores/              # Pinia state
    └── public/                  # Assets estáticos
```

---

## 🆕 Novedades en VersionM

### Auditoría de Productos
Se agregó la entidad `EliminacionProducto` para registrar:
- Cuándo se eliminó un producto
- Quién lo eliminó (Admin)
- Precio, código y stock al momento de eliminación
- Motivo de la eliminación
- IP desde donde se realizó la acción

### Mejoras de Serialización
- JSON ahora es case-insensitive
- Compatible con convenciones camelCase del Frontend

---

## 🛠️ Notas Técnicas

- **Framework Backend:** ASP.NET Core 8.0
- **Framework Frontend:** Vue 3 + Vite
- **Base de Datos:** PostgreSQL 17
- **ORM:** Entity Framework Core 8.0
- **Autenticación:** JWT con BCrypt
- **Estado:** Pinia (Vue)
- **HTTP Client:** Axios

---

## 📋 Documentación Completa

Para más detalles sobre la configuración, ver: **`CONFIGURACION_BD.md`**

---

## ✨ Verificación Rápida

Una vez que ejecutes los servidores:

1. Accede a `https://localhost:56397/` (Swagger Backend)
2. Accede a `http://localhost:3001/` (Frontend)
3. Intenta hacer login con `Sofia63@gmail.com` / `Password123!`
4. Navega por Clientes, Productos y Ventas

Si todo funciona, ¡estás listo! 🚀

---

**Fecha de configuración:** 10 de diciembre de 2025
**Estado:** ✅ Completamente funcional
