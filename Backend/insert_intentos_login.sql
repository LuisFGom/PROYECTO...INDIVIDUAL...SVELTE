-- Insert test login attempts
INSERT INTO "IntentosLogin" ("NombreUsuario", "Exitoso", "FechaIntento", "MensajeError", "IpAddress", "UserAgent")
VALUES 
  ('admin', true, NOW() - INTERVAL '2 days', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('admin', true, NOW() - INTERVAL '1 day', NULL, '192.168.1.1', 'Mozilla/5.0'),
  ('vendedor1', false, NOW() - INTERVAL '2 days', 'Contraseña incorrecta', '192.168.1.2', 'Chrome'),
  ('vendedor1', true, NOW() - INTERVAL '1 hour', NULL, '192.168.1.2', 'Chrome'),
  ('cajero1', true, NOW() - INTERVAL '30 minutes', NULL, '192.168.1.3', 'Firefox');

-- Verify
SELECT COUNT(*) as total_registros FROM "IntentosLogin";
SELECT * FROM "IntentosLogin" ORDER BY "FechaIntento" DESC;
