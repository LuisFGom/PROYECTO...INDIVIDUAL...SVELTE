using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities;
using PuntoVenta.Infrastructure.Persistencia;
using System.Text;

namespace PuntoVenta.Api.Controllers
{
    [Authorize(Roles = "Administrador,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class EliminacionesUsuariosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public EliminacionesUsuariosController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        /// <summary>
        /// Obtener todos los usuarios actualmente desactivados con su última eliminación registrada
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Consultar TODOS los usuarios sin filtros automáticos (IgnoreQueryFilters)
                var todosLosUsuarios = _context.Usuarios.IgnoreQueryFilters().AsNoTracking().ToList();
                var eliminaciones = await _unitOfWork.EliminacionesUsuarios.GetAllAsync();
                
                Console.WriteLine($"[DEBUG EliminacionesUsuariosController] Total usuarios en DB (todos sin filtros): {todosLosUsuarios.Count()}");
                Console.WriteLine($"[DEBUG EliminacionesUsuariosController] Total eliminaciones en DB: {eliminaciones.Count()}");
                
                // Obtener solo usuarios desactivados (Activo = false)
                var usuariosDesactivados = todosLosUsuarios.Where(u => !u.Activo).ToList();
                Console.WriteLine($"[DEBUG EliminacionesUsuariosController] Usuarios desactivados (Activo=false): {usuariosDesactivados.Count()}");
                
                foreach (var u in usuariosDesactivados)
                {
                    Console.WriteLine($"[DEBUG] Usuario desactivado: ID={u.Id}, Nombre={u.Nombre}, Activo={u.Activo}");
                }
                
                // Mapear usuarios desactivados con su última eliminación registrada
                var resultado = usuariosDesactivados.Select(u => {
                    var ultimaEliminacion = eliminaciones
                        .Where(e => e.UsuarioEliminadoId == u.Id)
                        .OrderByDescending(e => e.FechaEliminacion)
                        .FirstOrDefault();
                    
                    return new
                    {
                        id = ultimaEliminacion?.Id ?? 0,
                        usuarioEliminadoId = u.Id,
                        nombreUsuario = u.NombreUsuario,
                        nombre = u.Nombre,
                        apellido = u.Apellido ?? "",
                        email = u.Email,
                        emailUsuarioEliminado = u.Email,
                        rolNombre = u.RolNombre,
                        rolUsuarioEliminado = u.RolNombre,
                        nombreUsuarioEliminado = u.Nombre,
                        nombreAdministrador = ultimaEliminacion?.NombreAdministrador ?? "Sistema",
                        tipoEliminacion = ultimaEliminacion?.TipoEliminacion ?? "Desactivación",
                        fechaEliminacion = ultimaEliminacion?.FechaEliminacion ?? u.FechaCreacion,
                        motivoEliminacion = ultimaEliminacion?.MotivoEliminacion ?? ""
                    };
                }).OrderByDescending(e => e.fechaEliminacion).ToList();
                
                Console.WriteLine($"[DEBUG EliminacionesUsuariosController] Resultado a retornar: {resultado.Count()} registros");
                foreach (var r in resultado)
                {
                    Console.WriteLine($"[DEBUG] Resultado: usuarioEliminadoId={r.usuarioEliminadoId}, nombre={r.nombre}");
                }
                
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EliminacionesUsuariosController] Exception: {ex.Message}");
                Console.WriteLine($"[ERROR EliminacionesUsuariosController] StackTrace: {ex.StackTrace}");
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtener eliminación por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var eliminacion = await _unitOfWork.EliminacionesUsuarios.GetByIdAsync(id);
                if (eliminacion == null)
                {
                    return NotFound(new { mensaje = "Registro de eliminación no encontrado" });
                }
                return Ok(eliminacion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Buscar eliminaciones por término
        /// </summary>
        [HttpGet("buscar/{termino}")]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var eliminaciones = await _unitOfWork.EliminacionesUsuarios.BuscarAsync(termino);
                return Ok(eliminaciones);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtener eliminaciones por rango de fechas
        /// </summary>
        [HttpGet("por-fecha")]
        public async Task<IActionResult> GetPorFecha([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            try
            {
                var eliminaciones = await _unitOfWork.EliminacionesUsuarios.ObtenerPorFechaAsync(desde, hasta);
                return Ok(eliminaciones);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Generar PDF con el reporte de eliminaciones
        /// </summary>
        [HttpGet("pdf")]
        public async Task<IActionResult> GenerarPDF([FromQuery] string? termino = null)
        {
            try
            {
                IEnumerable<EliminacionUsuario> eliminaciones;
                
                if (!string.IsNullOrEmpty(termino))
                {
                    eliminaciones = await _unitOfWork.EliminacionesUsuarios.BuscarAsync(termino);
                }
                else
                {
                    eliminaciones = await _unitOfWork.EliminacionesUsuarios.GetAllAsync();
                }

                // Generar HTML para el PDF
                var html = GenerarHTMLReporte(eliminaciones.ToList());
                
                // Devolver como HTML para que el frontend lo convierta a PDF
                return Ok(new { 
                    html = html,
                    nombreArchivo = $"Reporte_Eliminaciones_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtener estadísticas de eliminaciones
        /// </summary>
        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                var eliminaciones = await _unitOfWork.EliminacionesUsuarios.GetAllAsync();
                var lista = eliminaciones.ToList();

                var estadisticas = new
                {
                    totalEliminaciones = lista.Count,
                    eliminacionesHoy = lista.Count(e => e.FechaEliminacion.Date == DateTime.UtcNow.Date),
                    eliminacionesSemana = lista.Count(e => e.FechaEliminacion >= DateTime.UtcNow.AddDays(-7)),
                    eliminacionesMes = lista.Count(e => e.FechaEliminacion >= DateTime.UtcNow.AddMonths(-1)),
                    porTipo = lista.GroupBy(e => e.TipoEliminacion)
                        .Select(g => new { tipo = g.Key, cantidad = g.Count() }),
                    porRol = lista.GroupBy(e => e.RolUsuarioEliminado)
                        .Select(g => new { rol = g.Key, cantidad = g.Count() }),
                    ultimasEliminaciones = lista.Take(5).Select(e => new
                    {
                        e.NombreUsuarioEliminado,
                        e.FechaEliminacion,
                        e.NombreAdministrador
                    })
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private string GenerarHTMLReporte(List<EliminacionUsuario> eliminaciones)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine(@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Reporte de Eliminaciones de Usuarios</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    h1 { color: #2c3e50; text-align: center; }
                    .info { text-align: center; color: #7f8c8d; margin-bottom: 20px; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th { background-color: #3a2a1f; color: white; padding: 12px; text-align: left; }
                    td { padding: 10px; border-bottom: 1px solid #ddd; }
                    tr:nth-child(even) { background-color: #f9f9f9; }
                    .footer { text-align: center; margin-top: 30px; color: #7f8c8d; font-size: 12px; }
                    .total { font-weight: bold; margin-top: 20px; }
                </style>
            </head>
            <body>
                <h1>📋 Reporte de Eliminaciones de Usuarios</h1>
                <p class='info'>Sistema de Facturación - Punto de Venta</p>
                <p class='info'>Generado el: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Fecha</th>
                            <th>Usuario Eliminado</th>
                            <th>Cédula</th>
                            <th>Email</th>
                            <th>Rol</th>
                            <th>Eliminado por</th>
                            <th>Tipo</th>
                            <th>Motivo</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var e in eliminaciones)
            {
                sb.AppendLine($@"
                        <tr>
                            <td>{e.FechaEliminacion:dd/MM/yyyy HH:mm}</td>
                            <td>{e.NombreUsuarioEliminado}</td>
                            <td>{e.CedulaUsuarioEliminado}</td>
                            <td>{e.EmailUsuarioEliminado}</td>
                            <td>{e.RolUsuarioEliminado}</td>
                            <td>{e.NombreAdministrador}</td>
                            <td>{e.TipoEliminacion}</td>
                            <td>{e.MotivoEliminacion ?? "-"}</td>
                        </tr>");
            }

            sb.AppendLine($@"
                    </tbody>
                </table>
                
                <p class='total'>Total de registros: {eliminaciones.Count}</p>
                
                <div class='footer'>
                    <p>Sistema de Facturación - Punto de Venta © {DateTime.Now.Year}</p>
                </div>
            </body>
            </html>");

            return sb.ToString();
        }
    }
}
