using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Options;

namespace PuntoVenta.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_smtpSettings.SmtpHost) || _smtpSettings.SmtpPort <= 0)
                {
                    _logger.LogWarning("SmtpSettings no configurados");
                    return;
                }

                using (var message = new MailMessage(_smtpSettings.SenderEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    using (var client = new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
                    {
                        Credentials = new NetworkCredential(_smtpSettings.SmtpUsername, _smtpSettings.SmtpPassword),
                        EnableSsl = true,
                        Timeout = 10000
                    })
                    {
                        _logger.LogInformation($"Enviando email a {toEmail}");
                        await client.SendMailAsync(message);
                        _logger.LogInformation($"Email enviado a {toEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviar email a {toEmail}");
            }
        }

        public async Task SendClienteCreatedEmailAsync(string clienteEmail, string clienteNombre, string clienteDocumento)
        {
            try
            {
                string subject = "Bienvenido a PuntoVenta";
                string htmlBody = GetWelcomeEmailHtml(clienteNombre, clienteDocumento, clienteEmail);
                await SendEmailAsync(clienteEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error email bienvenida a {clienteEmail}");
            }
        }

        public async Task SendClienteUpdatedEmailAsync(string clienteEmail, string clienteNombre, string camposModificados)
        {
            try
            {
                string subject = "Tu perfil ha sido actualizado";
                string htmlBody = GetUpdateEmailHtml(clienteNombre, camposModificados);
                await SendEmailAsync(clienteEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error email actualizacion a {clienteEmail}");
            }
        }

        public async Task SendClienteDeletedEmailAsync(string clienteEmail, string clienteNombre, string clienteDocumento)
        {
            try
            {
                string subject = "Tu cuenta ha sido desactivada";
                string htmlBody = GetDeleteEmailHtml(clienteNombre, clienteDocumento, clienteEmail);
                await SendEmailAsync(clienteEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error email desactivacion a {clienteEmail}");
            }
        }

        private string GetWelcomeEmailHtml(string nombre, string documento, string email)
        {
            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; }}
.container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; }}
.header {{ background: linear-gradient(135deg, #3B82F6, #1E40AF); color: white; padding: 20px; }}
</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>Bienvenido a PuntoVenta</h1></div>
<p>Hola {0},</p>
<p>Tu cuenta ha sido creada exitosamente.</p>
<p><strong>Nombre:</strong> {0}</p>
<p><strong>Documento:</strong> {1}</p>
<p><strong>Email:</strong> {2}</p>
<p><strong>Fecha:</strong> {3:dd/MM/yyyy HH:mm:ss}</p>
<p>Puedes acceder a tu cuenta desde el sistema.</p>
</div>
</body>
</html>", nombre, documento, email, DateTime.UtcNow);
        }

        private string GetUpdateEmailHtml(string nombre, string camposModificados)
        {
            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; }}
.container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; }}
.header {{ background: linear-gradient(135deg, #10B981, #047857); color: white; padding: 20px; }}
</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>Perfil Actualizado</h1></div>
<p>Hola {0},</p>
<p>Tu perfil ha sido actualizado en PuntoVenta.</p>
<p><strong>Campos modificados:</strong></p>
<ul>{1}</ul>
<p><strong>Fecha:</strong> {2:dd/MM/yyyy HH:mm:ss}</p>
</div>
</body>
</html>", nombre, camposModificados, DateTime.UtcNow);
        }

        private string GetDeleteEmailHtml(string nombre, string documento, string email)
        {
            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; }}
.container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; }}
.header {{ background: linear-gradient(135deg, #EF4444, #DC2626); color: white; padding: 20px; }}
</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>Cuenta Desactivada</h1></div>
<p>Hola {0},</p>
<p>Tu cuenta en PuntoVenta ha sido desactivada.</p>
<p><strong>Nombre:</strong> {0}</p>
<p><strong>Documento:</strong> {1}</p>
<p><strong>Email:</strong> {2}</p>
<p><strong>Fecha:</strong> {3:dd/MM/yyyy HH:mm:ss}</p>
<p>Si deseas reactivarla contacta soporte.</p>
</div>
</body>
</html>", nombre, documento, email, DateTime.UtcNow);
        }

        public async Task SendVentaCreatedEmailAsync(string clienteEmail, string clienteNombre, string numeroFactura, decimal totalVenta, string usuarioNombre)
        {
            try
            {
                string subject = "Tu venta ha sido registrada en PuntoVenta";
                string htmlBody = GetVentaCreatedEmailHtml(clienteNombre, numeroFactura, totalVenta, usuarioNombre);
                await SendEmailAsync(clienteEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error email de venta a {clienteEmail}");
            }
        }

        public async Task SendVentaCreatedWithDetailsEmailAsync(string clienteEmail, string clienteNombre, string numeroFactura, 
            decimal subtotal, decimal iva, decimal totalVenta, string usuarioNombre, List<(string nombre, int cantidad, decimal precioUnitario, decimal total)> detalles, 
            string? observaciones = null)
        {
            try
            {
                string subject = "Tu venta ha sido registrada en PuntoVenta";
                string htmlBody = GetVentaCreatedWithDetailsEmailHtml(clienteNombre, numeroFactura, subtotal, iva, totalVenta, usuarioNombre, detalles, observaciones);
                await SendEmailAsync(clienteEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error email de venta con detalles a {clienteEmail}");
            }
        }

        private string GetVentaCreatedEmailHtml(string nombre, string numeroFactura, decimal totalVenta, string usuarioNombre)
        {
            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; }}
.container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; }}
.header {{ background: linear-gradient(135deg, #8B5CF6, #7C3AED); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
.content {{ padding: 20px 0; }}
.details {{ background-color: #f9fafb; padding: 15px; border-radius: 8px; margin: 15px 0; }}
.detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e5e7eb; }}
.detail-row:last-child {{ border-bottom: none; }}
.label {{ font-weight: bold; color: #374151; }}
.value {{ color: #6b7280; }}
.total {{ font-size: 18px; font-weight: bold; color: #8B5CF6; margin-top: 15px; }}
.footer {{ color: #6b7280; font-size: 12px; margin-top: 20px; }}
</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>Venta Registrada</h1></div>
<div class='content'>
<p>Hola {0},</p>
<p>Te confirmamos que tu venta ha sido registrada exitosamente en el sistema.</p>
<div class='details'>
<div class='detail-row'>
<span class='label'>Número de Factura:</span>
<span class='value'>{1}</span>
</div>
<div class='detail-row'>
<span class='label'>Total de Venta:</span>
<span class='value'>${2:N2}</span>
</div>
<div class='detail-row'>
<span class='label'>Vendedor:</span>
<span class='value'>{3}</span>
</div>
<div class='detail-row'>
<span class='label'>Fecha:</span>
<span class='value'>{4:dd/MM/yyyy HH:mm:ss}</span>
</div>
<div class='total'>
Total: ${2:N2}
</div>
</div>
<p>Puedes acceder a los detalles de tu venta desde el sistema en cualquier momento.</p>
<p>Gracias por usar PuntoVenta.</p>
</div>
<div class='footer'>
<p>Este es un correo automático, por favor no respondas a este mensaje.</p>
</div>
</div>
</body>
</html>", nombre, numeroFactura, totalVenta, usuarioNombre, DateTime.UtcNow);
        }

        private string GetVentaCreatedWithDetailsEmailHtml(string nombre, string numeroFactura, decimal subtotal, decimal iva, 
            decimal totalVenta, string usuarioNombre, List<(string nombre, int cantidad, decimal precioUnitario, decimal total)> detalles, string? observaciones)
        {
            // Construir tabla de detalles
            string detallesHtml = string.Empty;
            if (detalles != null && detalles.Count > 0)
            {
                detallesHtml = "<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>";
                detallesHtml += "<thead style='background-color: #f3f4f6; border-bottom: 2px solid #e5e7eb;'>";
                detallesHtml += "<tr>";
                detallesHtml += "<th style='padding: 10px; text-align: left; font-weight: bold;'>Producto</th>";
                detallesHtml += "<th style='padding: 10px; text-align: center; font-weight: bold;'>Cantidad</th>";
                detallesHtml += "<th style='padding: 10px; text-align: right; font-weight: bold;'>Precio Unit.</th>";
                detallesHtml += "<th style='padding: 10px; text-align: right; font-weight: bold;'>Subtotal</th>";
                detallesHtml += "</tr>";
                detallesHtml += "</thead>";
                detallesHtml += "<tbody>";

                foreach (var detalle in detalles)
                {
                    detallesHtml += "<tr style='border-bottom: 1px solid #e5e7eb;'>";
                    detallesHtml += $"<td style='padding: 10px;'>{detalle.nombre}</td>";
                    detallesHtml += $"<td style='padding: 10px; text-align: center;'>{detalle.cantidad}</td>";
                    detallesHtml += $"<td style='padding: 10px; text-align: right;'>${detalle.precioUnitario:N2}</td>";
                    detallesHtml += $"<td style='padding: 10px; text-align: right; font-weight: bold;'>${detalle.total:N2}</td>";
                    detallesHtml += "</tr>";
                }

                detallesHtml += "</tbody>";
                detallesHtml += "</table>";
            }

            // Construir observaciones
            string observacionesHtml = string.Empty;
            if (!string.IsNullOrWhiteSpace(observaciones))
            {
                observacionesHtml = $"<div style='background-color: #FEF3C7; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #F59E0B;'>";
                observacionesHtml += "<p style='margin: 0;'><strong>Observaciones:</strong></p>";
                observacionesHtml += $"<p style='margin: 5px 0 0 0;'>{observaciones}</p>";
                observacionesHtml += "</div>";
            }

            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; }}
.container {{ max-width: 700px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
.header {{ background: linear-gradient(135deg, #8B5CF6, #7C3AED); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
.content {{ padding: 20px 0; }}
.section {{ margin: 20px 0; }}
.section-title {{ font-size: 16px; font-weight: bold; color: #1F2937; margin-bottom: 10px; border-bottom: 2px solid #E5E7EB; padding-bottom: 8px; }}
.details-table {{ background-color: #f9fafb; padding: 15px; border-radius: 8px; margin: 15px 0; }}
.detail-row {{ display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #e5e7eb; }}
.detail-row:last-child {{ border-bottom: none; }}
.label {{ font-weight: 600; color: #374151; }}
.value {{ color: #6b7280; }}
.totals-section {{ background-color: #f3f4f6; padding: 15px; border-radius: 8px; margin: 15px 0; }}
.total-row {{ display: flex; justify-content: space-between; padding: 10px 0; font-size: 15px; }}
.total-row.iva {{ color: #6b7280; }}
.total-row.final {{ font-size: 18px; font-weight: bold; color: #8B5CF6; border-top: 2px solid #e5e7eb; padding-top: 10px; }}
.footer {{ color: #6b7280; font-size: 12px; margin-top: 20px; text-align: center; }}
.info-box {{ background-color: #EFF6FF; padding: 12px; border-radius: 6px; border-left: 4px solid #3B82F6; margin: 15px 0; }}
</style>
</head>
<body>
<div class='container'>
<div class='header'><h1>✓ Venta Registrada Exitosamente</h1></div>
<div class='content'>
<p>Hola <strong>{nombre}</strong>,</p>
<p>Te confirmamos que tu venta ha sido registrada exitosamente en el sistema PuntoVenta.</p>

<div class='section'>
<div class='section-title'>Información de la Venta</div>
<div class='details-table'>
<div class='detail-row'>
<span class='label'>Número de Factura:</span>
<span class='value' style='font-family: monospace; font-weight: bold;'>{numeroFactura}</span>
</div>
<div class='detail-row'>
<span class='label'>Vendedor:</span>
<span class='value'>{usuarioNombre}</span>
</div>
<div class='detail-row'>
<span class='label'>Fecha y Hora:</span>
<span class='value'>{DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}</span>
</div>
</div>
</div>

<div class='section'>
<div class='section-title'>Detalle de Productos</div>
{detallesHtml}
</div>

<div class='section'>
<div class='section-title'>Resumen de Totales</div>
<div class='totals-section'>
<div class='total-row'>
<span class='label'>Subtotal:</span>
<span class='value'>${subtotal:N2}</span>
</div>
<div class='total-row iva'>
<span class='label'>IVA (19%):</span>
<span class='value'>${iva:N2}</span>
</div>
<div class='total-row final'>
<span class='label'>TOTAL:</span>
<span class='value'>${totalVenta:N2}</span>
</div>
</div>
</div>

{observacionesHtml}

<div class='info-box'>
<p style='margin: 0;'><strong>ℹ️ Información importante:</strong></p>
<p style='margin: 5px 0 0 0; font-size: 13px;'>Puedes acceder a los detalles completos de tu venta desde el sistema en cualquier momento. Conserva este correo para tu registro.</p>
</div>

<p>Gracias por usar <strong>PuntoVenta</strong>.</p>
</div>
<div class='footer'>
<p>Este es un correo automático. Por favor, no respondas a este mensaje.</p>
<p style='margin-top: 10px; color: #9CA3AF;'>© 2026 PuntoVenta - Todos los derechos reservados</p>
</div>
</div>
</body>
</html>";
        }
    }
}
