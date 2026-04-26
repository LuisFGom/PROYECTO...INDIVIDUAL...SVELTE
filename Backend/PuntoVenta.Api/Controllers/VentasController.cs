using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.DTOs;
using PuntoVenta.Application.Features.Ventas.Commands;
using PuntoVenta.Application.Features.Ventas.Queries;
using PuntoVenta.Application.Features.Auditorias.Commands;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PuntoVenta.Api.Controllers
{
    /// <summary>
    /// Controlador para gestionar operaciones de ventas (facturas)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public VentasController(IMediator mediator, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        /// <summary>
        /// Obtiene todas las ventas con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<VentaResponseDto>>> GetVentas(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] int? clienteId = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                // Obtener todas las facturas directamente desde el repositorio
                var facturas = await _unitOfWork.Facturas.GetAllAsync();
                
                // Aplicar filtros
                var query = facturas.AsEnumerable();

                if (fechaInicio.HasValue)
                {
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value);
                }

                if (usuarioId.HasValue)
                {
                    query = query.Where(v => v.UsuarioId == usuarioId.Value);
                }

                if (clienteId.HasValue)
                {
                    query = query.Where(v => v.ClienteId == clienteId.Value);
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(v => v.Estado == estado);
                }

                // Mapear a DTO - Usar hash code del ObjectId como int temporal para compatibilidad
                var result = query.Select(f => new VentaResponseDto
                {
                    VentaId = f.Id,
                    NumeroFactura = f.NumeroFactura,
                    FechaVenta = f.FechaVenta,
                    UsuarioId = f.UsuarioId,
                    UsuarioNombre = f.UsuarioNombre,
                    ClienteId = f.ClienteId,
                    ClienteNombre = f.ClienteNombre,
                    Subtotal = f.Subtotal,
                    PorcentajeIVA = f.PorcentajeIVA,
                    TotalImpuesto = f.TotalImpuesto,
                    TotalVenta = f.TotalVenta,
                    Estado = f.Estado,
                    Observaciones = f.Observaciones
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el total de ventas del mes actual
        /// </summary>
        [HttpGet("mes-actual")]
        public async Task<ActionResult<int>> GetVentasMesActual()
        {
            try
            {
                var facturas = await _unitOfWork.Facturas.GetAllAsync();
                var mesActual = DateTime.UtcNow.Month;
                var anioActual = DateTime.UtcNow.Year;
                
                var ventasMes = facturas.Count(f => 
                    f.FechaVenta.Month == mesActual && 
                    f.FechaVenta.Year == anioActual &&
                    f.Estado != "Cancelada" &&
                    f.Estado != "Anulada"
                );
                
                return Ok(ventasMes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint temporal de diagnóstico para inspeccionar directamente las facturas almacenadas
        /// </summary>
        [HttpGet("debug/raw")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFacturasRaw()
        {
            var facturas = await _unitOfWork.Facturas.GetAllAsync();
            return Ok(new
            {
                Count = facturas?.Count() ?? 0,
                Datos = facturas
            });
        }

        /// <summary>
        /// Obtiene una venta específica por ID con todos sus detalles
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDetailResponseDto>> GetVentaById(int id)
        {
            try
            {
                var factura = await _unitOfWork.Facturas.GetFacturaConDetallesAsync(id);
                if (factura == null)
                {
                    return NotFound(new { message = $"Factura con ID {id} no encontrada" });
                }

                return Ok(MapFacturaToDetailDto(factura));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva venta (factura)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateVenta([FromBody] CreateVentaDto createVentaDto)
        {
            try
            {
                // Obtener ID del usuario desde el token JWT
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var command = new CreateVentaCommand
                {
                    UsuarioId = usuarioId,
                    ClienteId = createVentaDto.ClienteId,
                    Detalles = createVentaDto.Detalles,
                    Observaciones = createVentaDto.Observaciones
                };

                var result = await _mediator.Send(command);
                
                // Enviar correo al cliente
                try
                {
                    // Obtener datos de la venta creada
                    var venta = await _unitOfWork.Facturas.GetByIdAsync(result);
                    if (venta != null && venta.ClienteId.HasValue)
                    {
                        // Obtener datos del cliente
                        var cliente = await _unitOfWork.Clientes.GetByIdAsync(venta.ClienteId.Value);
                        
                        // Obtener datos del usuario (vendedor)
                        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                        
                        if (cliente != null && !string.IsNullOrWhiteSpace(cliente.Email))
                        {
                            string clienteNombre = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                            string usuarioNombre = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}".Trim() : "Vendedor";
                            
                            // Construir lista de detalles para el correo
                            var detallesEmail = new List<(string nombre, int cantidad, decimal precioUnitario, decimal total)>();
                            
                            if (venta.Detalles != null && venta.Detalles.Count > 0)
                            {
                                foreach (var detalle in venta.Detalles)
                                {
                                    detallesEmail.Add((
                                        nombre: detalle.ProductoNombre,
                                        cantidad: detalle.Cantidad,
                                        precioUnitario: detalle.PrecioUnitario,
                                        total: detalle.Total
                                    ));
                                }
                            }
                            
                            // Calcular subtotal e IVA (19%)
                            decimal subtotal = venta.Subtotal;
                            decimal iva = venta.TotalImpuesto;
                            
                            await _emailService.SendVentaCreatedWithDetailsEmailAsync(
                                cliente.Email,
                                clienteNombre,
                                venta.NumeroFactura,
                                subtotal,
                                iva,
                                venta.TotalVenta,
                                usuarioNombre,
                                detallesEmail,
                                venta.Observaciones
                            );
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    // Log del error pero no fallar la venta
                    Console.WriteLine($"Error enviando correo de venta: {emailEx.Message}");
                }
                
                return CreatedAtAction(nameof(GetVentaById), new { id = result }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el estado y observaciones de una venta existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<bool>> UpdateVenta(int id, [FromBody] UpdateVentaCommand updateCommand)
        {
            try
            {
                updateCommand.VentaId = id;
                var result = await _mediator.Send(updateCommand);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una venta (solo si está en estado Cancelada o Anulada)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<bool>> DeleteVenta(int id)
        {
            try
            {
                var command = new DeleteVentaCommand { VentaId = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza solo las cantidades de los productos en una factura
        /// </summary>
        [HttpPut("{id}/cantidad")]
        [Authorize]
        public async Task<ActionResult<bool>> UpdateVentaCantidad(int id, [FromBody] UpdateVentaCantidadCommand command)
        {
            try
            {
                command.VentaId = id;
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Marca una factura como eliminada (soft delete) y restaura stock
        /// </summary>
        [HttpPut("{id}/eliminar")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteVentaSoft(int id)
        {
            try
            {
                var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "desconocido";
                var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var apellido = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
                var usuarioNombre = $"{nombre} {apellido}".Trim();

                var command = new DeleteVentaSoftCommand 
                { 
                    VentaId = id,
                    UsuarioId = usuarioId,
                    NombreUsuario = usuarioNombre
                };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las facturas marcadas como eliminadas
        /// </summary>
        [HttpGet("eliminadas/lista")]
        [Authorize]
        public async Task<ActionResult<List<VentaResponseDto>>> GetEliminacionesVentas(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? clienteId = null,
            [FromQuery] string? numeroFactura = null)
        {
            try
            {
                var query = new GetEliminacionesVentasQuery
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    ClienteId = clienteId,
                    NumeroFactura = numeroFactura
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Reactiva una factura que fue marcada como eliminada y restaura el stock
        /// </summary>
        [HttpPut("{id}/reinsertar")]
        [Authorize]
        public async Task<ActionResult<bool>> ReinsertatVenta(int id)
        {
            try
            {
                var command = new ReinsertatVentaCommand { VentaId = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Genera PDF de una factura
        /// </summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetVentaPDF(int id)
        {
            try
            {
                var factura = await _unitOfWork.Facturas.GetFacturaConDetallesAsync(id);
                if (factura == null)
                {
                    return NotFound(new { message = $"Factura con ID {id} no encontrada" });
                }

                // Generar PDF simple con información de la factura
                var pdfBytes = GenerarPDFFactura(factura);

                return File(pdfBytes, "application/pdf", $"Factura_{factura.NumeroFactura}.pdf");
            }
            catch (OperationCanceledException)
            {
                // Cliente canceló la descarga - no es un error
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar PDF", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una venta por número de factura
        /// </summary>
        [HttpGet("numero/{numeroFactura}")]
        public async Task<ActionResult<VentaDetailResponseDto>> GetVentaPorNumero(string numeroFactura)
        {
            try
            {
                var factura = await _unitOfWork.Facturas.GetByNumeroFacturaAsync(numeroFactura);
                if (factura == null)
                {
                    return NotFound(new { message = $"Factura con número {numeroFactura} no encontrada" });
                }

                return Ok(MapFacturaToDetailDto(factura));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Genera el PDF de una factura con base en su número
        /// </summary>
        [HttpGet("numero/{numeroFactura}/pdf")]
        public async Task<IActionResult> GetVentaPDFPorNumero(string numeroFactura)
        {
            try
            {
                Console.WriteLine($"[PDF] === INICIO SOLICITUD PDF ===");
                Console.WriteLine($"[PDF] Numero de factura solicitado: {numeroFactura}");
                
                var factura = await _unitOfWork.Facturas.GetByNumeroFacturaAsync(numeroFactura);
                if (factura == null)
                {
                    Console.WriteLine($"[PDF] ERROR: Factura no encontrada: {numeroFactura}");
                    return NotFound(new { message = $"Factura con número {numeroFactura} no encontrada" });
                }

                Console.WriteLine($"[PDF] Factura encontrada. ID: {factura.Id}, Detalles: {factura.Detalles?.Count ?? 0}");
                Console.WriteLine($"[PDF] Llamando a GenerarPDFFactura...");
                
                byte[] pdfBytes;
                try
                {
                    pdfBytes = GenerarPDFFactura(factura);
                    Console.WriteLine($"[PDF] PDF generado correctamente. Tamaño: {pdfBytes.Length} bytes");
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"[PDF ERROR en GenerarPDFFactura] {pdfEx.GetType().Name}: {pdfEx.Message}");
                    Console.WriteLine($"[PDF ERROR StackTrace] {pdfEx.StackTrace}");
                    throw;
                }
                
                Console.WriteLine($"[PDF] Preparando respuesta HTTP...");
                
                // Establecer headers explícitamente (usando Append para .NET 8)
                var fileName = $"Factura_{factura.NumeroFactura}.pdf";
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Append("Content-Length", pdfBytes.Length.ToString());
                Response.ContentType = "application/pdf";
                
                Console.WriteLine($"[PDF] Headers establecidos. Enviando {pdfBytes.Length} bytes...");
                
                // Escribir directamente al response stream
                await Response.Body.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                await Response.Body.FlushAsync();
                
                Console.WriteLine($"[PDF] === FIN SOLICITUD PDF (EXITOSO) ===");
                
                // Retornar vacío ya que escribimos directamente al response
                return new EmptyResult();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"[PDF] Operación cancelada por el cliente: {ex.Message}");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[PDF ERROR] InvalidOperationException: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Error al generar PDF", 
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF ERROR FATAL] Tipo: {ex.GetType().Name}");
                Console.WriteLine($"[PDF ERROR FATAL] Mensaje: {ex.Message}");
                Console.WriteLine($"[PDF ERROR FATAL] StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[PDF ERROR Inner] Tipo: {ex.InnerException.GetType().Name}");
                    Console.WriteLine($"[PDF ERROR Inner] Mensaje: {ex.InnerException.Message}");
                    Console.WriteLine($"[PDF ERROR Inner] StackTrace: {ex.InnerException.StackTrace}");
                }
                
                Console.WriteLine($"[PDF] === FIN SOLICITUD PDF (CON ERROR) ===");
                
                return StatusCode(500, new { 
                    message = "Error al generar PDF", 
                    details = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        private static VentaDetailResponseDto MapFacturaToDetailDto(Factura factura)
        {
            return new VentaDetailResponseDto
            {
                VentaId = factura.Id,
                NumeroFactura = factura.NumeroFactura,
                FechaVenta = factura.FechaVenta,
                UsuarioId = factura.UsuarioId,
                UsuarioNombre = factura.UsuarioNombre,
                ClienteId = factura.ClienteId,
                ClienteNombre = factura.ClienteNombre,
                Subtotal = factura.Subtotal,
                PorcentajeIVA = factura.PorcentajeIVA,
                TotalImpuesto = factura.TotalImpuesto,
                TotalVenta = factura.TotalVenta,
                Estado = factura.Estado,
                Observaciones = factura.Observaciones,
                Detalles = factura.Detalles?.Select(d => new DetalleVentaResponseDto
                {
                    VentaId = factura.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.ProductoNombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    Total = d.Total
                }).ToList() ?? new List<DetalleVentaResponseDto>()
            };
        }

        private byte[]? GenerarPDFFactura(Factura factura)
        {
            byte[]? pdfBytes = null;
            
            try
            {
                Console.WriteLine("[PDF] Iniciando generación de PDF...");
                
                // Validar que la factura tenga datos
                if (factura == null)
                {
                    throw new ArgumentNullException(nameof(factura), "La factura no puede ser null");
                }

                try
                {
                    Console.WriteLine("[PDF] Configurando licencia de QuestPDF...");
                    // Configurar licencia de QuestPDF (Community license para uso no comercial)
                    QuestPDF.Settings.License = LicenseType.Community;
                    Console.WriteLine("[PDF] Licencia configurada correctamente");
                }
                catch (Exception licenseEx)
                {
                    Console.WriteLine($"[PDF ERROR] Error configurando licencia: {licenseEx.Message}");
                    throw;
                }
                
                try
                {
                    Console.WriteLine("[PDF] Creando documento PDF...");
                    
                    var document = Document.Create(container =>
                    {
                        try
                        {
                            container.Page(page =>
                            {
                                page.Size(PageSizes.A4);
                                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                                page.DefaultTextStyle(x => x.FontSize(11));

                                // Header
                                page.Header().Element(ComposeHeader);

                                // Content
                                page.Content().Element(container => ComposeContent(container, factura));

                                // Footer
                                page.Footer().AlignCenter().Text(text =>
                                {
                                    text.Span("Gracias por su compra | ");
                                    text.Span($"Página ").FontSize(9);
                                    text.CurrentPageNumber().FontSize(9);
                                    text.Span(" de ").FontSize(9);
                                    text.TotalPages().FontSize(9);
                                });
                            });
                        }
                        catch (Exception pageEx)
                        {
                            Console.WriteLine($"[PDF ERROR] Error al construir página: {pageEx.GetType().Name}: {pageEx.Message}");
                            throw;
                        }
                    });

                    Console.WriteLine("[PDF] Documento creado. Generando bytes del PDF...");
                    
                    // Generar PDF en memoria y obtener los bytes
                    using (var stream = new MemoryStream())
                    {
                        try
                        {
                            Console.WriteLine("[PDF] Llamando a GeneratePdf...");
                            document.GeneratePdf(stream);
                            Console.WriteLine("[PDF] GeneratePdf completado");
                            
                            pdfBytes = stream.ToArray();
                            Console.WriteLine($"[PDF] PDF generado correctamente. Tamaño: {pdfBytes.Length} bytes");
                        }
                        catch (Exception genEx)
                        {
                            Console.WriteLine($"[PDF ERROR] Error generando PDF bytes: {genEx.GetType().Name}: {genEx.Message}");
                            Console.WriteLine($"[PDF ERROR StackTrace] {genEx.StackTrace}");
                            throw;
                        }
                    }
                }
                catch (Exception docEx)
                {
                    Console.WriteLine($"[PDF ERROR] Error en Document.Create: {docEx.GetType().Name}: {docEx.Message}");
                    Console.WriteLine($"[PDF ERROR StackTrace] {docEx.StackTrace}");
                    throw;
                }
                
                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF ERROR CRITICO en GenerarPDFFactura] Tipo: {ex.GetType().Name}");
                Console.WriteLine($"[PDF ERROR CRITICO en GenerarPDFFactura] Mensaje: {ex.Message}");
                Console.WriteLine($"[PDF ERROR CRITICO en GenerarPDFFactura] StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[PDF ERROR CRITICO Inner] Mensaje: {ex.InnerException.Message}");
                    Console.WriteLine($"[PDF ERROR CRITICO Inner] StackTrace: {ex.InnerException.StackTrace}");
                }
                
                throw new InvalidOperationException($"Error al generar PDF: {ex.Message}", ex);
            }
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("SISTEMA DE FACTURACIÓN")
                        .FontSize(20)
                        .Bold()
                        .FontColor("#FF7713");

                    column.Item().Text("Punto de Venta")
                        .FontSize(12)
                        .FontColor("#180A01");
                });

                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text("FACTURA")
                        .FontSize(18)
                        .Bold()
                        .FontColor("#180A01");
                });
            });
        }

        void ComposeContent(IContainer container, Factura factura)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(10);

                // Información de la factura
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Número de Factura: {factura.NumeroFactura ?? "N/A"}").Bold();
                        col.Item().Text($"Fecha: {factura.FechaVenta:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Estado: {factura.Estado ?? "N/A"}");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Cliente: {factura.ClienteNombre ?? "Consumidor Final"}").Bold();
                        col.Item().Text($"Atendido por: {factura.UsuarioNombre ?? "N/A"}");
                    });
                });

                column.Item().LineHorizontal(1).LineColor("#FF7713");

                // Tabla de productos
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Producto
                        columns.RelativeColumn(1); // Cantidad
                        columns.RelativeColumn(1); // P. Unit
                        columns.RelativeColumn(1); // Total
                    });

                    // Header de la tabla
                    table.Header(header =>
                    {
                        header.Cell().Background("#FF7713").Padding(5)
                            .Text("Producto").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#FF7713").Padding(5)
                            .Text("Cant.").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#FF7713").Padding(5)
                            .Text("P. Unit.").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#FF7713").Padding(5)
                            .Text("Total").FontColor("#FFFFFF").Bold();
                    });

                    // Detalles de productos
                    if (factura.Detalles != null && factura.Detalles.Any())
                    {
                        foreach (var detalle in factura.Detalles)
                        {
                            if (detalle == null) continue; // Saltar detalles nulos
                            
                            table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(5)
                                .Text(detalle.ProductoNombre ?? "Producto sin nombre");
                            table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(5)
                                .AlignCenter().Text(detalle.Cantidad.ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(5)
                                .AlignRight().Text($"${detalle.PrecioUnitario:F2}");
                            table.Cell().BorderBottom(0.5f).BorderColor("#E0E0E0").Padding(5)
                                .AlignRight().Text($"${detalle.Total:F2}").Bold();
                        }
                    }
                    else
                    {
                        // Fila vacía si no hay detalles
                        table.Cell().ColumnSpan(4).Padding(10)
                            .AlignCenter().Text("Sin productos").FontSize(10).Italic();
                    }
                });

                // Totales
                column.Item().PaddingTop(10).AlignRight().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(150).Text("Subtotal:");
                        row.AutoItem().Width(100).AlignRight().Text($"${factura.Subtotal:F2}");
                    });

                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(150).Text($"IVA ({factura.PorcentajeIVA:F0}%):");
                        row.AutoItem().Width(100).AlignRight().Text($"${factura.TotalImpuesto:F2}");
                    });

                    col.Item().LineHorizontal(1).LineColor("#FF7713");

                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(150).Text("TOTAL:").Bold().FontSize(14);
                        row.AutoItem().Width(100).AlignRight().Text($"${factura.TotalVenta:F2}")
                            .Bold().FontSize(14).FontColor("#FF7713");
                    });
                });

                // Observaciones
                if (!string.IsNullOrEmpty(factura.Observaciones))
                {
                    column.Item().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("Observaciones:").Bold();
                        col.Item().Text(factura.Observaciones).FontSize(10);
                    });
                }
            });
        }
    }
}
