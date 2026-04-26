-- Insertar roles si no existen
INSERT INTO roles (nombre, descripcion, activo) 
SELECT 'Admin', 'Administrador del sistema', true
WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre = 'Admin');

INSERT INTO roles (nombre, descripcion, activo) 
SELECT 'Vendedor', 'Usuario vendedor', true
WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre = 'Vendedor');

INSERT INTO roles (nombre, descripcion, activo) 
SELECT 'Cajero', 'Operario de caja', true
WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre = 'Cajero');

INSERT INTO roles (nombre, descripcion, activo) 
SELECT 'Gerente', 'Gerente de ventas', true
WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre = 'Gerente');

INSERT INTO roles (nombre, descripcion, activo) 
SELECT 'Auditor', 'Auditor del sistema', true
WHERE NOT EXISTS (SELECT 1 FROM roles WHERE nombre = 'Auditor');
