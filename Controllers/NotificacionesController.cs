using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionAgricola.Data;
using SistemaGestionAgricola.Models.Entities;
using SistemaGestionAgricola.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ← PROTECCIÓN AGREGADA
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificacionesController> _logger;

        public NotificacionesController(AppDbContext context, ILogger<NotificacionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Notificaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificacionDTO>>> GetNotificaciones(
            [FromQuery] string? tipo = null,
            [FromQuery] string? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Notificacion> query = _context.Notificaciones;

                // Si no es admin, solo ver sus notificaciones
                if (currentUserRole != "admin")
                {
                    query = query.Where(n => n.UsuarioId == currentUserId);
                }

                query = query
                    .Include(n => n.Usuario)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .AsQueryable();

                // Filtros
                if (!string.IsNullOrEmpty(tipo))
                    query = query.Where(n => n.Tipo == tipo.ToLower());

                if (!string.IsNullOrEmpty(estado))
                    query = query.Where(n => n.Estado == estado.ToLower());

                if (fechaDesde.HasValue)
                    query = query.Where(n => n.FechaProgramada >= fechaDesde.Value.Date);

                if (fechaHasta.HasValue)
                    query = query.Where(n => n.FechaProgramada <= fechaHasta.Value.Date);

                // Paginación
                var totalRecords = await query.CountAsync();
                var notificaciones = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificacionDTO
                    {
                        Id = n.Id,
                        UsuarioId = n.UsuarioId,
                        CultivoId = n.CultivoId,
                        Tipo = n.Tipo,
                        Mensaje = n.Mensaje,
                        FechaProgramada = n.FechaProgramada,
                        Estado = n.Estado,
                        FechaEnvio = n.FechaEnvio,
                        CreatedAt = n.CreatedAt,
                        UsuarioNombre = n.Usuario.Nombre,
                        UsuarioEmail = n.Usuario.Email,
                        CultivoNombre = n.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = n.Cultivo.Terreno.Nombre,
                        AgricultorNombre = n.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Respuesta con metadatos de paginación
                var response = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Data = notificaciones
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Notificaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificacionDTO>> GetNotificacion(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.Notificaciones
                    .Include(n => n.Usuario)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Where(n => n.Id == id)
                    .Select(n => new NotificacionDTO
                    {
                        Id = n.Id,
                        UsuarioId = n.UsuarioId,
                        CultivoId = n.CultivoId,
                        Tipo = n.Tipo,
                        Mensaje = n.Mensaje,
                        FechaProgramada = n.FechaProgramada,
                        Estado = n.Estado,
                        FechaEnvio = n.FechaEnvio,
                        CreatedAt = n.CreatedAt,
                        UsuarioNombre = n.Usuario.Nombre,
                        UsuarioEmail = n.Usuario.Email,
                        CultivoNombre = n.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = n.Cultivo.Terreno.Nombre,
                        AgricultorNombre = n.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (notificacion == null)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                return notificacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificación con ID {NotificacionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Notificaciones/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<NotificacionDTO>>> GetNotificacionesByUsuario(
            int usuarioId,
            [FromQuery] bool soloPendientes = false)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos
                if (currentUserRole != "admin" && usuarioId != currentUserId)
                    return Forbid();

                var query = _context.Notificaciones
                    .Include(n => n.Usuario)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Where(n => n.UsuarioId == usuarioId);

                if (soloPendientes)
                {
                    query = query.Where(n => n.Estado == "pendiente");
                }

                var notificaciones = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificacionDTO
                    {
                        Id = n.Id,
                        UsuarioId = n.UsuarioId,
                        CultivoId = n.CultivoId,
                        Tipo = n.Tipo,
                        Mensaje = n.Mensaje,
                        FechaProgramada = n.FechaProgramada,
                        Estado = n.Estado,
                        FechaEnvio = n.FechaEnvio,
                        CreatedAt = n.CreatedAt,
                        UsuarioNombre = n.Usuario.Nombre,
                        UsuarioEmail = n.Usuario.Email,
                        CultivoNombre = n.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = n.Cultivo.Terreno.Nombre,
                        AgricultorNombre = n.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Notificaciones/pendientes
        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<NotificacionDTO>>> GetNotificacionesPendientes()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Notificacion> query = _context.Notificaciones
                    .Where(n => n.Estado == "pendiente" && n.FechaProgramada <= DateTime.Today);

                // Si no es admin, solo ver sus notificaciones pendientes
                if (currentUserRole != "admin")
                {
                    query = query.Where(n => n.UsuarioId == currentUserId);
                }

                var notificaciones = await query
                    .Include(n => n.Usuario)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(n => n.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .OrderBy(n => n.FechaProgramada)
                    .Select(n => new NotificacionDTO
                    {
                        Id = n.Id,
                        UsuarioId = n.UsuarioId,
                        CultivoId = n.CultivoId,
                        Tipo = n.Tipo,
                        Mensaje = n.Mensaje,
                        FechaProgramada = n.FechaProgramada,
                        Estado = n.Estado,
                        FechaEnvio = n.FechaEnvio,
                        CreatedAt = n.CreatedAt,
                        UsuarioNombre = n.Usuario.Nombre,
                        UsuarioEmail = n.Usuario.Email,
                        CultivoNombre = n.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = n.Cultivo.Terreno.Nombre,
                        AgricultorNombre = n.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones pendientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Notificaciones
        [HttpPost]
        public async Task<ActionResult<NotificacionDTO>> PostNotificacion(CreateNotificacionDTO createNotificacionDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validación del modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar permisos - solo admin puede crear notificaciones para otros usuarios
                if (createNotificacionDTO.UsuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                // Verificar si el usuario existe
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == createNotificacionDTO.UsuarioId);
                
                if (usuario == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                // Verificar si el cultivo existe
                var cultivo = await _context.Cultivos
                    .Include(c => c.TipoCultivo)
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == createNotificacionDTO.CultivoId);
                
                if (cultivo == null)
                {
                    return BadRequest("El cultivo especificado no existe");
                }

                var notificacion = new Notificacion
                {
                    UsuarioId = createNotificacionDTO.UsuarioId,
                    CultivoId = createNotificacionDTO.CultivoId,
                    Tipo = createNotificacionDTO.Tipo.Trim().ToLower(),
                    Mensaje = createNotificacionDTO.Mensaje.Trim(),
                    FechaProgramada = createNotificacionDTO.FechaProgramada.Date,
                    Estado = createNotificacionDTO.Estado?.Trim().ToLower() ?? "pendiente"
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                var notificacionDTO = new NotificacionDTO
                {
                    Id = notificacion.Id,
                    UsuarioId = notificacion.UsuarioId,
                    CultivoId = notificacion.CultivoId,
                    Tipo = notificacion.Tipo,
                    Mensaje = notificacion.Mensaje,
                    FechaProgramada = notificacion.FechaProgramada,
                    Estado = notificacion.Estado,
                    FechaEnvio = notificacion.FechaEnvio,
                    CreatedAt = notificacion.CreatedAt,
                    UsuarioNombre = usuario.Nombre,
                    UsuarioEmail = usuario.Email,
                    CultivoNombre = cultivo.TipoCultivo.Nombre,
                    TerrenoNombre = cultivo.Terreno.Nombre,
                    AgricultorNombre = cultivo.Terreno.Agricultor.Usuario.Nombre
                };

                _logger.LogInformation("Notificación creada con ID {NotificacionId}", notificacion.Id);

                return CreatedAtAction(nameof(GetNotificacion), new { id = notificacion.Id }, notificacionDTO);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al crear notificación");
                return StatusCode(500, "Error al guardar en la base de datos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Notificaciones/5/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstadoNotificacion(int id, UpdateNotificacionDTO updateDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.Notificaciones.FindAsync(id);
                if (notificacion == null)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }

                // Verificar permisos - solo el dueño de la notificación o admin puede actualizarla
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                if (!string.IsNullOrEmpty(updateDto.Estado))
                {
                    notificacion.Estado = updateDto.Estado.Trim().ToLower();

                    // Si se marca como enviada, registrar fecha de envío
                    if (updateDto.Estado.ToLower() == "enviada" && !notificacion.FechaEnvio.HasValue)
                    {
                        notificacion.FechaEnvio = DateTime.UtcNow;
                    }
                }

                if (updateDto.FechaEnvio.HasValue)
                {
                    notificacion.FechaEnvio = updateDto.FechaEnvio.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de notificación {NotificacionId} actualizado a {Estado}", id, updateDto.Estado);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de notificación {NotificacionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Notificaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotificacion(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.Notificaciones.FindAsync(id);
                if (notificacion == null)
                {
                    return NotFound($"Notificación con ID {id} no encontrada");
                }

                // Verificar permisos - solo el dueño de la notificación o admin puede eliminarla
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                _context.Notificaciones.Remove(notificacion);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notificación {NotificacionId} eliminada", id);

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al eliminar notificación {NotificacionId}", id);
                return StatusCode(500, "Error al eliminar en la base de datos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación {NotificacionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool NotificacionExists(int id)
        {
            return _context.Notificaciones.Any(e => e.Id == id);
        }

        // Métodos de validación
        private bool IsValidTipo(string tipo)
        {
            var tiposValidos = new[] { "riego", "fumigacion", "cosecha", "alerta", "recordatorio" };
            return tiposValidos.Contains(tipo.ToLower());
        }

        private bool IsValidEstado(string estado)
        {
            var estadosValidos = new[] { "pendiente", "enviada", "leida", "cancelada" };
            return estadosValidos.Contains(estado.ToLower());
        }
    }
}