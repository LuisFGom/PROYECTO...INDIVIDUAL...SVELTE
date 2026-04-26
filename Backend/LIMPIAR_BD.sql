-- ============================================================================
-- SCRIPT DE LIMPIEZA DE DATOS SUCIOS EN LA BD - PuntoVenta
-- Versión PostgreSQL con CamelCase de columnas (entre comillas)
-- ============================================================================

-- 1. LIMPIAR USUARIOS - Eliminar registros con datos incompletos y duplicados
-- ============================================================================

-- Mostrar usuarios con problemas
SELECT "Id", "NombreUsuario", "Email", "Nombre", "Apellido", "FechaCreacion" 
FROM usuarios 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Apellido" IS NULL OR "Apellido" = '' OR "Apellido" = 'N/A'
   OR "Email" IS NULL OR "Email" = '' OR "Email" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por email (mantener el primero)
DELETE FROM usuarios 
WHERE "Id" IN (
    SELECT u1."Id" FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2."Email") = LOWER(u1."Email") 
        AND u2."Id" < u1."Id"
    )
);

-- Eliminar duplicados por nombre_usuario (mantener el primero)
DELETE FROM usuarios 
WHERE "Id" IN (
    SELECT u1."Id" FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2."NombreUsuario") = LOWER(u1."NombreUsuario") 
        AND u2."Id" < u1."Id"
    )
);

-- Normalizar emails a minúsculas
UPDATE usuarios SET "Email" = LOWER(TRIM("Email")) WHERE "Email" IS NOT NULL;

-- Normalizar nombres y apellidos (trim)
UPDATE usuarios SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;
UPDATE usuarios SET "Apellido" = TRIM("Apellido") WHERE "Apellido" IS NOT NULL;
UPDATE usuarios SET "NombreUsuario" = TRIM("NombreUsuario") WHERE "NombreUsuario" IS NOT NULL;


-- 2. LIMPIAR CLIENTES - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar clientes con problemas
SELECT "Id", "Nombre", "Documento", "Email", "Telefono", "FechaCreacion" 
FROM clientes 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Documento" IS NULL OR "Documento" = '' OR "Documento" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por documento (mantener el primero)
DELETE FROM clientes 
WHERE "Id" IN (
    SELECT c1."Id" FROM clientes c1
    WHERE EXISTS (
        SELECT 1 FROM clientes c2 
        WHERE LOWER(c2."Documento") = LOWER(c1."Documento") 
        AND c2."Id" < c1."Id"
    )
);

-- Normalizar documentos (trim)
UPDATE clientes SET "Documento" = TRIM("Documento") WHERE "Documento" IS NOT NULL;

-- Normalizar teléfonos y emails
UPDATE clientes SET "Telefono" = TRIM("Telefono") WHERE "Telefono" IS NOT NULL AND "Telefono" != 'N/A';
UPDATE clientes SET "Email" = LOWER(TRIM("Email")) WHERE "Email" IS NOT NULL AND "Email" != 'N/A';  
UPDATE clientes SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;


-- 3. LIMPIAR PRODUCTOS - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar productos con problemas
SELECT "Id", "Nombre", "Codigo", "Descripcion", "Precio", "FechaCreacion" 
FROM productos 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Codigo" IS NULL OR "Codigo" = '' OR "Codigo" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por código (mantener el primero)
DELETE FROM productos 
WHERE "Id" IN (
    SELECT p1."Id" FROM productos p1
    WHERE EXISTS (
        SELECT 1 FROM productos p2 
        WHERE LOWER(p2."Codigo") = LOWER(p1."Codigo") 
        AND p2."Id" < p1."Id"
    )
);

-- Normalizar códigos (trim y mayúsculas)
UPDATE productos SET "Codigo" = UPPER(TRIM("Codigo")) WHERE "Codigo" IS NOT NULL;

-- Normalizar nombres y descripciones
UPDATE productos SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;
UPDATE productos SET "Descripcion" = TRIM("Descripcion") WHERE "Descripcion" IS NOT NULL AND "Descripcion" != 'N/A';


-- 4. VERIFICAR INTEGRIDAD DESPUÉS DE LA LIMPIEZA
-- ============================================================================

-- Contar registros limpios
SELECT 
    (SELECT COUNT(*) FROM usuarios) as total_usuarios,
    (SELECT COUNT(*) FROM clientes) as total_clientes,
    (SELECT COUNT(*) FROM productos) as total_productos,
    (SELECT COUNT(*) FROM facturas) as total_facturas;

-- Verificar que no haya duplicados de email en usuarios
SELECT "Email", COUNT(*) as repeticiones 
FROM usuarios 
WHERE "Email" IS NOT NULL 
GROUP BY LOWER("Email") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de usuario en usuarios
SELECT "NombreUsuario", COUNT(*) as repeticiones 
FROM usuarios 
WHERE "NombreUsuario" IS NOT NULL 
GROUP BY LOWER("NombreUsuario") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de documento en clientes
SELECT "Documento", COUNT(*) as repeticiones 
FROM clientes 
WHERE "Documento" IS NOT NULL 
GROUP BY LOWER("Documento") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de código en productos
SELECT "Codigo", COUNT(*) as repeticiones 
FROM productos 
WHERE "Codigo" IS NOT NULL 
GROUP BY LOWER("Codigo") 
HAVING COUNT(*) > 1;

-- Eliminar duplicados por email (mantener el primero)
DELETE FROM usuarios 
WHERE id IN (
    SELECT u1.id FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2.email) = LOWER(u1.email) 
        AND u2.id < u1.id
    )
);

-- Eliminar duplicados por nombre_usuario (mantener el primero)
DELETE FROM usuarios 
WHERE id IN (
    SELECT u1.id FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2.nombre_usuario) = LOWER(u1.nombre_usuario) 
        AND u2.id < u1.id
    )
);

-- Normalizar emails a minúsculas
UPDATE usuarios SET email = LOWER(TRIM(email)) WHERE email IS NOT NULL;

-- Normalizar nombres y apellidos (trim)
UPDATE usuarios SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;
UPDATE usuarios SET apellido = TRIM(apellido) WHERE apellido IS NOT NULL;
UPDATE usuarios SET nombre_usuario = TRIM(nombre_usuario) WHERE nombre_usuario IS NOT NULL;


-- 2. LIMPIAR CLIENTES - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar clientes con problemas
SELECT id, nombre, documento, email, telefono, fecha_creacion 
FROM clientes 
WHERE nombre IS NULL OR nombre = '' OR nombre = 'N/A'
   OR documento IS NULL OR documento = '' OR documento = 'N/A'
ORDER BY fecha_creacion DESC;

-- Eliminar duplicados por documento (mantener el primero)
DELETE FROM clientes 
WHERE id IN (
    SELECT c1.id FROM clientes c1
    WHERE EXISTS (
        SELECT 1 FROM clientes c2 
        WHERE LOWER(c2.documento) = LOWER(c1.documento) 
        AND c2.id < c1.id
    )
);

-- Normalizar documentos (trim)
UPDATE clientes SET documento = TRIM(documento) WHERE documento IS NOT NULL;

-- Normalizar teléfonos y emails
UPDATE clientes SET telefono = TRIM(telefono) WHERE telefono IS NOT NULL AND telefono != 'N/A';
UPDATE clientes SET email = LOWER(TRIM(email)) WHERE email IS NOT NULL AND email != 'N/A';  
UPDATE clientes SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;


-- 3. LIMPIAR PRODUCTOS - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar productos con problemas
SELECT id, nombre, codigo, descripcion, precio, fecha_creacion 
FROM productos 
WHERE nombre IS NULL OR nombre = '' OR nombre = 'N/A'
   OR codigo IS NULL OR codigo = '' OR codigo = 'N/A'
ORDER BY fecha_creacion DESC;

-- Eliminar duplicados por código (mantener el primero)
DELETE FROM productos 
WHERE id IN (
    SELECT p1.id FROM productos p1
    WHERE EXISTS (
        SELECT 1 FROM productos p2 
        WHERE LOWER(p2.codigo) = LOWER(p1.codigo) 
        AND p2.id < p1.id
    )
);

-- Normalizar códigos (trim y mayúsculas)
UPDATE productos SET codigo = UPPER(TRIM(codigo)) WHERE codigo IS NOT NULL;

-- Normalizar nombres y descripciones
UPDATE productos SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;
UPDATE productos SET descripcion = TRIM(descripcion) WHERE descripcion IS NOT NULL AND descripcion != 'N/A';


-- 4. VERIFICAR INTEGRIDAD DESPUÉS DE LA LIMPIEZA
-- ============================================================================

-- Contar registros limpios
SELECT 
    (SELECT COUNT(*) FROM usuarios) as total_usuarios,
    (SELECT COUNT(*) FROM clientes) as total_clientes,
    (SELECT COUNT(*) FROM productos) as total_productos,
    (SELECT COUNT(*) FROM facturas) as total_facturas;

-- Verificar que no haya duplicados de email en usuarios
SELECT email, COUNT(*) as repeticiones 
FROM usuarios 
WHERE email IS NOT NULL 
GROUP BY LOWER(email) 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de usuario en usuarios
SELECT nombre_usuario, COUNT(*) as repeticiones 
FROM usuarios 
WHERE nombre_usuario IS NOT NULL 
GROUP BY LOWER(nombre_usuario) 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de documento en clientes
SELECT documento, COUNT(*) as repeticiones 
FROM clientes 
WHERE documento IS NOT NULL 
GROUP BY LOWER(documento) 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de código en productos
SELECT codigo, COUNT(*) as repeticiones 
FROM productos 
WHERE codigo IS NOT NULL 
GROUP BY LOWER(codigo) 
HAVING COUNT(*) > 1; 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Apellido" IS NULL OR "Apellido" = '' OR "Apellido" = 'N/A'
   OR "Email" IS NULL OR "Email" = '' OR "Email" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por email (mantener el primero)
DELETE FROM "Usuarios" 
WHERE "Id" IN (
    SELECT u1."Id" FROM "Usuarios" u1
    WHERE EXISTS (
        SELECT 1 FROM "Usuarios" u2 
        WHERE LOWER(u2."Email") = LOWER(u1."Email") 
        AND u2."Id" < u1."Id"
    )
);

-- Eliminar duplicados por nombre_usuario (mantener el primero)
DELETE FROM "Usuarios" 
WHERE "Id" IN (
    SELECT u1."Id" FROM "Usuarios" u1
    WHERE EXISTS (
        SELECT 1 FROM "Usuarios" u2 
        WHERE LOWER(u2."NombreUsuario") = LOWER(u1."NombreUsuario") 
        AND u2."Id" < u1."Id"
    )
);

-- Normalizar emails a minúsculas
UPDATE "Usuarios" SET "Email" = LOWER(TRIM("Email")) WHERE "Email" IS NOT NULL;

-- Normalizar nombres y apellidos (trim)
UPDATE "Usuarios" SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;
UPDATE "Usuarios" SET "Apellido" = TRIM("Apellido") WHERE "Apellido" IS NOT NULL;
UPDATE "Usuarios" SET "NombreUsuario" = TRIM("NombreUsuario") WHERE "NombreUsuario" IS NOT NULL;


-- 2. LIMPIAR CLIENTES - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar clientes con problemas
SELECT "Id", "Nombre", "Documento", "Email", "Telefono", "FechaCreacion" 
FROM "Clientes" 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Documento" IS NULL OR "Documento" = '' OR "Documento" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por documento (mantener el primero)
DELETE FROM "Clientes" 
WHERE "Id" IN (
    SELECT c1."Id" FROM "Clientes" c1
    WHERE EXISTS (
        SELECT 1 FROM "Clientes" c2 
        WHERE LOWER(c2."Documento") = LOWER(c1."Documento") 
        AND c2."Id" < c1."Id"
    )
);

-- Normalizar documentos (trim)
UPDATE "Clientes" SET "Documento" = TRIM("Documento") WHERE "Documento" IS NOT NULL;

-- Normalizar teléfonos y emails
UPDATE "Clientes" SET "Telefono" = TRIM("Telefono") WHERE "Telefono" IS NOT NULL AND "Telefono" != 'N/A';
UPDATE "Clientes" SET "Email" = LOWER(TRIM("Email")) WHERE "Email" IS NOT NULL AND "Email" != 'N/A';  
UPDATE "Clientes" SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;


-- 3. LIMPIAR PRODUCTOS - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Mostrar productos con problemas
SELECT "Id", "Nombre", "Codigo", "Descripcion", "Precio", "FechaCreacion" 
FROM "Productos" 
WHERE "Nombre" IS NULL OR "Nombre" = '' OR "Nombre" = 'N/A'
   OR "Codigo" IS NULL OR "Codigo" = '' OR "Codigo" = 'N/A'
ORDER BY "FechaCreacion" DESC;

-- Eliminar duplicados por código (mantener el primero)
DELETE FROM "Productos" 
WHERE "Id" IN (
    SELECT p1."Id" FROM "Productos" p1
    WHERE EXISTS (
        SELECT 1 FROM "Productos" p2 
        WHERE LOWER(p2."Codigo") = LOWER(p1."Codigo") 
        AND p2."Id" < p1."Id"
    )
);

-- Normalizar códigos (trim y mayúsculas)
UPDATE "Productos" SET "Codigo" = UPPER(TRIM("Codigo")) WHERE "Codigo" IS NOT NULL;

-- Normalizar nombres y descripciones
UPDATE "Productos" SET "Nombre" = TRIM("Nombre") WHERE "Nombre" IS NOT NULL;
UPDATE "Productos" SET "Descripcion" = TRIM("Descripcion") WHERE "Descripcion" IS NOT NULL AND "Descripcion" != 'N/A';


-- 4. VERIFICAR INTEGRIDAD DESPUÉS DE LA LIMPIEZA
-- ============================================================================

-- Contar registros limpios
SELECT 
    (SELECT COUNT(*) FROM "Usuarios") as total_usuarios,
    (SELECT COUNT(*) FROM "Clientes") as total_clientes,
    (SELECT COUNT(*) FROM "Productos") as total_productos,
    (SELECT COUNT(*) FROM "Facturas") as total_facturas;

-- Verificar que no haya duplicados de email en usuarios
SELECT "Email", COUNT(*) as repeticiones 
FROM "Usuarios" 
WHERE "Email" IS NOT NULL 
GROUP BY LOWER("Email") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de usuario en usuarios
SELECT "NombreUsuario", COUNT(*) as repeticiones 
FROM "Usuarios" 
WHERE "NombreUsuario" IS NOT NULL 
GROUP BY LOWER("NombreUsuario") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de documento en clientes
SELECT "Documento", COUNT(*) as repeticiones 
FROM "Clientes" 
WHERE "Documento" IS NOT NULL 
GROUP BY LOWER("Documento") 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de código en productos
SELECT "Codigo", COUNT(*) as repeticiones 
FROM "Productos" 
WHERE "Codigo" IS NOT NULL 
GROUP BY LOWER("Codigo") 
HAVING COUNT(*) > 1;

-- 1. LIMPIAR USUARIOS - Eliminar registros con datos incompletos y duplicados
-- ============================================================================

-- Verificar usuarios con problemas
SELECT id, nombre_usuario, email, nombre, apellido, created_at 
FROM usuarios 
WHERE nombre IS NULL OR nombre = '' OR nombre = 'N/A'
   OR apellido IS NULL OR apellido = '' OR apellido = 'N/A'
   OR email IS NULL OR email = '' OR email = 'N/A'
ORDER BY created_at DESC;

-- Eliminar duplicados por email (mantener el primero)
DELETE FROM usuarios 
WHERE id IN (
    SELECT id FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2.email) = LOWER(u1.email) 
        AND u2.id < u1.id
    )
);

-- Eliminar duplicados por nombre_usuario (mantener el primero)
DELETE FROM usuarios 
WHERE id IN (
    SELECT id FROM usuarios u1
    WHERE EXISTS (
        SELECT 1 FROM usuarios u2 
        WHERE LOWER(u2.nombre_usuario) = LOWER(u1.nombre_usuario) 
        AND u2.id < u1.id
    )
);

-- Normalizar emails a minúsculas
UPDATE usuarios SET email = LOWER(TRIM(email)) WHERE email IS NOT NULL;

-- Normalizar nombres y apellidos (trim y capitalizar)
UPDATE usuarios SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;
UPDATE usuarios SET apellido = TRIM(apellido) WHERE apellido IS NOT NULL;
UPDATE usuarios SET nombre_usuario = TRIM(nombre_usuario) WHERE nombre_usuario IS NOT NULL;


-- 2. LIMPIAR CLIENTES - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Verificar clientes con problemas
SELECT id, nombre, documento, email, telefono, created_at 
FROM clientes 
WHERE nombre IS NULL OR nombre = '' OR nombre = 'N/A'
   OR documento IS NULL OR documento = '' OR documento = 'N/A'
ORDER BY created_at DESC;

-- Eliminar duplicados por documento (mantener el primero)
DELETE FROM clientes 
WHERE id IN (
    SELECT id FROM clientes c1
    WHERE EXISTS (
        SELECT 1 FROM clientes c2 
        WHERE LOWER(c2.documento) = LOWER(c1.documento) 
        AND c2.id < c1.id
    )
);

-- Normalizar documentos (trim)
UPDATE clientes SET documento = TRIM(documento) WHERE documento IS NOT NULL;

-- Normalizar teléfonos y emails
UPDATE clientes SET telefono = TRIM(telefono) WHERE telefono IS NOT NULL AND telefono != 'N/A';
UPDATE clientes SET email = LOWER(TRIM(email)) WHERE email IS NOT NULL AND email != 'N/A';
UPDATE clientes SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;


-- 3. LIMPIAR PRODUCTOS - Eliminar datos incompletos y duplicados
-- ============================================================================

-- Verificar productos con problemas
SELECT id, nombre, codigo, descripcion, precio, created_at 
FROM productos 
WHERE nombre IS NULL OR nombre = '' OR nombre = 'N/A'
   OR codigo IS NULL OR codigo = '' OR codigo = 'N/A'
ORDER BY created_at DESC;

-- Eliminar duplicados por código (mantener el primero)
DELETE FROM productos 
WHERE id IN (
    SELECT id FROM productos p1
    WHERE EXISTS (
        SELECT 1 FROM productos p2 
        WHERE LOWER(p2.codigo) = LOWER(p1.codigo) 
        AND p2.id < p1.id
    )
);

-- Normalizar códigos (trim y mayúsculas)
UPDATE productos SET codigo = UPPER(TRIM(codigo)) WHERE codigo IS NOT NULL;

-- Normalizar nombres y descripciones
UPDATE productos SET nombre = TRIM(nombre) WHERE nombre IS NOT NULL;
UPDATE productos SET descripcion = TRIM(descripcion) WHERE descripcion IS NOT NULL AND descripcion != 'N/A';


-- 4. VERIFICAR INTEGRIDAD DESPUÉS DE LA LIMPIEZA
-- ============================================================================

-- Contar registros limpios
SELECT 
    (SELECT COUNT(*) FROM usuarios) as total_usuarios,
    (SELECT COUNT(*) FROM clientes) as total_clientes,
    (SELECT COUNT(*) FROM productos) as total_productos,
    (SELECT COUNT(*) FROM facturas) as total_facturas;

-- Verificar que no haya duplicados de email en usuarios
SELECT email, COUNT(*) as repeticiones 
FROM usuarios 
WHERE email IS NOT NULL 
GROUP BY LOWER(email) 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de documento en clientes
SELECT documento, COUNT(*) as repeticiones 
FROM clientes 
WHERE documento IS NOT NULL 
GROUP BY LOWER(documento) 
HAVING COUNT(*) > 1;

-- Verificar que no haya duplicados de código en productos
SELECT codigo, COUNT(*) as repeticiones 
FROM productos 
WHERE codigo IS NOT NULL 
GROUP BY LOWER(codigo) 
HAVING COUNT(*) > 1;

-- ============================================================================
-- FIN DEL SCRIPT DE LIMPIEZA
-- ============================================================================
