using System.Threading.Tasks;
using System.Collections.Generic;

namespace PuntoVenta.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendClienteCreatedEmailAsync(string clienteEmail, string clienteNombre, string clienteDocumento);
        Task SendClienteUpdatedEmailAsync(string clienteEmail, string clienteNombre, string camposModificados);
        Task SendClienteDeletedEmailAsync(string clienteEmail, string clienteNombre, string clienteDocumento);
        Task SendVentaCreatedEmailAsync(string clienteEmail, string clienteNombre, string numeroFactura, decimal totalVenta, string usuarioNombre);
        Task SendVentaCreatedWithDetailsEmailAsync(string clienteEmail, string clienteNombre, string numeroFactura, 
            decimal subtotal, decimal iva, decimal totalVenta, string usuarioNombre, List<(string nombre, int cantidad, decimal precioUnitario, decimal total)> detalles, 
            string? observaciones = null);
    }
}