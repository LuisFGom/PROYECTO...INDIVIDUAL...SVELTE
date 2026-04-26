# 🧹 Guía de Limpieza de Datos - PuntoVenta

## Problema Identificado

El sistema tiene **datos sucios y duplicados** en la BD:
- ✗ Registros con valores "N/A"
- ✗ Usuarios/Clientes/Productos duplicados
- ✗ Emails y documentos sin normalizar
- ✗ Espacios en blanco sin trim

**Causa:** Datos históricos anteriores a las validaciones actuales del backend.

---

## ✅ Soluciones Implementadas

### 1. Backend Mejorado (HECHO ✓)
- ✅ **CreateUsuarioCommandHandler** - Ahora normaliza y valida:
  - Trim de espacios en blanco
  - Emails convertidos a minúsculas
  - Validación de campos no vacíos
  - Mensajes de error claros

- ✅ **UsuariosController** - Devuelve BadRequest (400) en lugar de 500

- ✅ **ApplicationDbContext** - Mantiene constraints UNIQUE en BD:
  - `Usuario.Email` UNIQUE ✓
  - `Usuario.NombreUsuario` UNIQUE ✓
  - `Cliente.Documento` UNIQUE ✓
  - `Producto.Codigo` UNIQUE ✓

### 2. Script de Limpieza (LISTO PARA EJECUTAR)
**Archivo:** `LIMPIAR_BD.sql`

---

## 🚀 Cómo Ejecutar la Limpieza

### Opción A: DBeaver (RECOMENDADO)
1. Abre DBeaver
2. Conéctate a PostgreSQL
3. Abre el archivo `LIMPIAR_BD.sql`
4. Haz clic en **Execute** o presiona `Ctrl+Enter`

### Opción B: pgAdmin
1. Abre pgAdmin → Base de datos `PuntoVenta`
2. Copia y pega el contenido de `LIMPIAR_BD.sql`
3. Ejecuta en pequeñas secciones (comentadas)

### Opción C: Command Line
```bash
psql -U postgres -d PuntoVenta -f LIMPIAR_BD.sql
```

---

## 📋 Qué Hace el Script

### Usuarios
- ✓ Elimina duplicados por email (mantiene el primero)
- ✓ Elimina duplicados por nombre_usuario
- ✓ Normaliza emails a minúsculas
- ✓ Trim de todos los campos de texto

### Clientes
- ✓ Elimina duplicados por documento
- ✓ Normaliza documentos, teléfonos, emails
- ✓ Trim de campos

### Productos
- ✓ Elimina duplicados por código
- ✓ Convierte códigos a MAYÚSCULAS
- ✓ Trim de campos

---

## ⚠️ IMPORTANTE ANTES DE EJECUTAR

### 1. HAZ UN BACKUP PRIMERO
```sql
-- En pgAdmin o DBeaver, crea un backup:
-- Menu: Tools → Backup → Full Backup de la BD PuntoVenta
```

### 2. VERIFICA EL SCRIPT
**El script primero MUESTRA qué va a eliminar:**

```sql
-- Líneas 19-24: Muestra usuarios con datos incompletos
-- Líneas 59-66: Muestra clientes con datos incompletos
-- Líneas 90-97: Muestra productos con datos incompletos
```

Copia y ejecuta SOLO ESTAS LÍNEAS primero para ver qué hay que limpiar.

### 3. EJECUTA EN ORDEN
1. Primero: Visualizar datos (`SELECT`)
2. Luego: Eliminar duplicados (`DELETE`)
3. Finalmente: Normalizar (`UPDATE`)

---

## 🔄 Verificación Post-Limpieza

Después de ejecutar, verifica que no haya duplicados:

```sql
-- Verificar NO HAY EMAIL DUPLICADOS
SELECT email, COUNT(*) FROM usuarios WHERE email IS NOT NULL 
GROUP BY LOWER(email) HAVING COUNT(*) > 1;
-- Resultado esperado: 0 filas

-- Verificar NO HAY USUARIO DUPLICADOS
SELECT nombre_usuario, COUNT(*) FROM usuarios WHERE nombre_usuario IS NOT NULL 
GROUP BY LOWER(nombre_usuario) HAVING COUNT(*) > 1;
-- Resultado esperado: 0 filas

-- Verificar NO HAY DOCUMENTO DUPLICADOS
SELECT documento, COUNT(*) FROM clientes WHERE documento IS NOT NULL 
GROUP BY LOWER(documento) HAVING COUNT(*) > 1;
-- Resultado esperado: 0 filas
```

---

## ✨ Después de la Limpieza

**Prueba en el Frontend:**
1. Recarga F5
2. Navega a Usuarios
3. Verifica que NO HAY más "N/A"
4. Intenta crear un usuario duplicado → Debe rechazar con error 400

**Prueba de Validación:**
```bash
# En Chrome DevTools Console:
# Intenta crear usuario con email existente
# La respuesta debería ser:
# {"mensaje": "Ya existe un usuario registrado con el email: ..."}
# Status: 400 (No 500)
```

---

## 📝 Próximos Pasos

Después de limpiar:
1. ✓ Limpiar Productos igual (aplicar mismo patrón)
2. ✓ Limpiar Clientes igual
3. ✓ Crear guía de validación en el frontend
4. ✓ Agregar validaciones en forms (HTML5 + JS)
5. ✓ Agregar feedback visual (modales de error)

---

## 🆘 Si Algo Sale Mal

Si hay error de constraint:
```
ERROR: duplicate key value violates unique constraint "usuarios_email_key"
```

**Solución:** Ejecuta el DELETE de duplicados ANTES de UPDATE:
1. Comenta las líneas de UPDATE
2. Ejecuta solo los DELETE
3. Luego sí los UPDATE

---

**¿Preguntas? Revisa los logs en Backend/logs/ o contacta soporte.**
