-- Script para limpiar todos los datos de prueba en PostgreSQL
-- ADVERTENCIA: Este script eliminará todos los datos de las tablas

BEGIN;

-- Desactivar restricciones de clave foránea temporalmente
ALTER TABLE detalles_venta DISABLE TRIGGER ALL;
ALTER TABLE facturas DISABLE TRIGGER ALL;
ALTER TABLE usuarios DISABLE TRIGGER ALL;
ALTER TABLE clientes DISABLE TRIGGER ALL;
ALTER TABLE productos DISABLE TRIGGER ALL;
ALTER TABLE intentos_login DISABLE TRIGGER ALL;
ALTER TABLE error_logs DISABLE TRIGGER ALL;
ALTER TABLE eliminaciones_usuarios DISABLE TRIGGER ALL;

-- Truncate tables
TRUNCATE TABLE detalles_venta CASCADE;
TRUNCATE TABLE facturas CASCADE;
TRUNCATE TABLE intentos_login CASCADE;
TRUNCATE TABLE error_logs CASCADE;
TRUNCATE TABLE eliminaciones_usuarios CASCADE;
TRUNCATE TABLE usuarios CASCADE;
TRUNCATE TABLE clientes CASCADE;
TRUNCATE TABLE productos CASCADE;
TRUNCATE TABLE roles CASCADE;

-- Reactivar restricciones
ALTER TABLE detalles_venta ENABLE TRIGGER ALL;
ALTER TABLE facturas ENABLE TRIGGER ALL;
ALTER TABLE usuarios ENABLE TRIGGER ALL;
ALTER TABLE clientes ENABLE TRIGGER ALL;
ALTER TABLE productos ENABLE TRIGGER ALL;
ALTER TABLE intentos_login ENABLE TRIGGER ALL;
ALTER TABLE error_logs ENABLE TRIGGER ALL;
ALTER TABLE eliminaciones_usuarios ENABLE TRIGGER ALL;

COMMIT;

-- Verificación de que las tablas están vacías
SELECT 'Roles' as tabla, COUNT(*) as registros FROM roles
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM usuarios
UNION ALL
SELECT 'Clientes', COUNT(*) FROM clientes
UNION ALL
SELECT 'Productos', COUNT(*) FROM productos
UNION ALL
SELECT 'Facturas', COUNT(*) FROM facturas
UNION ALL
SELECT 'Detalles Venta', COUNT(*) FROM detalles_venta
UNION ALL
SELECT 'Intentos Login', COUNT(*) FROM intentos_login
UNION ALL
SELECT 'Error Logs', COUNT(*) FROM error_logs;
