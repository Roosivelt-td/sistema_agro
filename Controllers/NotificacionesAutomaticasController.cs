using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionAgricola.Data;
using SistemaGestionAgricola.Models.Entities;
using SistemaGestionAgricola.Models.DTOs;

namespace SistemaGestionAgricola.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ← PROTECCIÓN AGREGADA
    public class NotificacionesAutomaticasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificacionesAutomaticasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/NotificacionesAutomaticas
        [HttpGet]
        [Authorize(Roles = "admin")] // ← Solo admin puede ver todas las configuraciones
        public async Task<ActionResult<IEnumerable<NotificacionAutomaticaDTO>>> GetNotificacionesAutomaticas()
        {
            try
            {
                var notificaciones = await _context.NotificacionesAutomaticas
                    .Include(na => na.TipoCultivo)
                    .Include(na => na.Usuario)
                    .Select(na => new NotificacionAutomaticaDTO
                    {
                        Id = na.Id,
                        TipoCultivoId = na.TipoCultivoId,
                        UsuarioId = na.UsuarioId,
                        TipoEvento = na.TipoEvento,
                        DiasDespuesSiembra = na.DiasDespuesSiembra,
                        Mensaje = na.Mensaje,
                        CreatedAt = na.CreatedAt,
                        TipoCultivoNombre = na.TipoCultivo.Nombre,
                        UsuarioNombre = na.Usuario.Nombre,
                        UsuarioEmail = na.Usuario.Email
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/NotificacionesAutomaticas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificacionAutomaticaDTO>> GetNotificacionAutomatica(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.NotificacionesAutomaticas
                    .Include(na => na.TipoCultivo)
                    .Include(na => na.Usuario)
                    .Where(na => na.Id == id)
                    .Select(na => new NotificacionAutomaticaDTO
                    {
                        Id = na.Id,
                        TipoCultivoId = na.TipoCultivoId,
                        UsuarioId = na.UsuarioId,
                        TipoEvento = na.TipoEvento,
                        DiasDespuesSiembra = na.DiasDespuesSiembra,
                        Mensaje = na.Mensaje,
                        CreatedAt = na.CreatedAt,
                        TipoCultivoNombre = na.TipoCultivo.Nombre,
                        UsuarioNombre = na.Usuario.Nombre,
                        UsuarioEmail = na.Usuario.Email
                    })
                    .FirstOrDefaultAsync();

                if (notificacion == null)
                {
                    return NotFound($"Notificación automática con ID {id} no encontrada");
                }

                // Verificar permisos - solo el dueño o admin puede ver
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                return notificacion;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/NotificacionesAutomaticas/tipocultivo/5
        [HttpGet("tipocultivo/{tipoCultivoId}")]
        public async Task<ActionResult<IEnumerable<NotificacionAutomaticaDTO>>> GetNotificacionesByTipoCultivo(int tipoCultivoId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<NotificacionAutomatica> query = _context.NotificacionesAutomaticas
                    .Where(na => na.TipoCultivoId == tipoCultivoId);

                // Si no es admin, solo ver sus configuraciones
                if (currentUserRole != "admin")
                {
                    query = query.Where(na => na.UsuarioId == currentUserId);
                }

                var notificaciones = await query
                    .Include(na => na.TipoCultivo)
                    .Include(na => na.Usuario)
                    .Select(na => new NotificacionAutomaticaDTO
                    {
                        Id = na.Id,
                        TipoCultivoId = na.TipoCultivoId,
                        UsuarioId = na.UsuarioId,
                        TipoEvento = na.TipoEvento,
                        DiasDespuesSiembra = na.DiasDespuesSiembra,
                        Mensaje = na.Mensaje,
                        CreatedAt = na.CreatedAt,
                        TipoCultivoNombre = na.TipoCultivo.Nombre,
                        UsuarioNombre = na.Usuario.Nombre,
                        UsuarioEmail = na.Usuario.Email
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/NotificacionesAutomaticas/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<NotificacionAutomaticaDTO>>> GetNotificacionesByUsuario(int usuarioId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos - solo puede ver sus propias configuraciones
                if (currentUserRole != "admin" && usuarioId != currentUserId)
                    return Forbid();

                var notificaciones = await _context.NotificacionesAutomaticas
                    .Include(na => na.TipoCultivo)
                    .Include(na => na.Usuario)
                    .Where(na => na.UsuarioId == usuarioId)
                    .Select(na => new NotificacionAutomaticaDTO
                    {
                        Id = na.Id,
                        TipoCultivoId = na.TipoCultivoId,
                        UsuarioId = na.UsuarioId,
                        TipoEvento = na.TipoEvento,
                        DiasDespuesSiembra = na.DiasDespuesSiembra,
                        Mensaje = na.Mensaje,
                        CreatedAt = na.CreatedAt,
                        TipoCultivoNombre = na.TipoCultivo.Nombre,
                        UsuarioNombre = na.Usuario.Nombre,
                        UsuarioEmail = na.Usuario.Email
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/NotificacionesAutomaticas
        [HttpPost]
        public async Task<ActionResult<NotificacionAutomaticaDTO>> PostNotificacionAutomatica(CreateNotificacionAutomaticaDTO createNotificacionDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createNotificacionDTO.TipoEvento))
                {
                    return BadRequest("TipoEvento es un campo requerido");
                }

                if (string.IsNullOrWhiteSpace(createNotificacionDTO.Mensaje))
                {
                    return BadRequest("Mensaje es un campo requerido");
                }

                // Validar tipo de evento
                if (!IsValidTipoEvento(createNotificacionDTO.TipoEvento))
                {
                    return BadRequest("Tipo de evento no válido. Los valores permitidos son: riego, fumigacion, cosecha, mantenimiento");
                }

                // Verificar permisos - solo puede crear configuraciones para sí mismo
                if (createNotificacionDTO.UsuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                // Verificar si el tipo de cultivo existe
                var tipoCultivo = await _context.TipoCultivos
                    .FirstOrDefaultAsync(tc => tc.Id == createNotificacionDTO.TipoCultivoId);

                if (tipoCultivo == null)
                {
                    return BadRequest("El tipo de cultivo especificado no existe");
                }

                // Verificar si el usuario existe
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == createNotificacionDTO.UsuarioId);

                if (usuario == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                var notificacion = new NotificacionAutomatica
                {
                    TipoCultivoId = createNotificacionDTO.TipoCultivoId,
                    UsuarioId = createNotificacionDTO.UsuarioId,
                    TipoEvento = createNotificacionDTO.TipoEvento.Trim(),
                    DiasDespuesSiembra = createNotificacionDTO.DiasDespuesSiembra,
                    Mensaje = createNotificacionDTO.Mensaje.Trim()
                };

                _context.NotificacionesAutomaticas.Add(notificacion);
                await _context.SaveChangesAsync();

                var notificacionDTO = new NotificacionAutomaticaDTO
                {
                    Id = notificacion.Id,
                    TipoCultivoId = notificacion.TipoCultivoId,
                    UsuarioId = notificacion.UsuarioId,
                    TipoEvento = notificacion.TipoEvento,
                    DiasDespuesSiembra = notificacion.DiasDespuesSiembra,
                    Mensaje = notificacion.Mensaje,
                    CreatedAt = notificacion.CreatedAt,
                    TipoCultivoNombre = tipoCultivo.Nombre,
                    UsuarioNombre = usuario.Nombre,
                    UsuarioEmail = usuario.Email
                };

                return CreatedAtAction(nameof(GetNotificacionAutomatica), new { id = notificacion.Id }, notificacionDTO);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: api/NotificacionesAutomaticas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotificacionAutomatica(int id, UpdateNotificacionAutomaticaDTO updateNotificacionDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.NotificacionesAutomaticas.FindAsync(id);

                if (notificacion == null)
                {
                    return NotFound($"Notificación automática con ID {id} no encontrada");
                }

                // Verificar permisos - solo el dueño o admin puede actualizar
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                // Validar tipo de evento si se está actualizando
                if (updateNotificacionDTO.TipoEvento != null && !IsValidTipoEvento(updateNotificacionDTO.TipoEvento))
                {
                    return BadRequest("Tipo de evento no válido. Los valores permitidos son: riego, fumigacion, cosecha, mantenimiento");
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateNotificacionDTO.TipoEvento != null)
                    notificacion.TipoEvento = updateNotificacionDTO.TipoEvento.Trim();

                if (updateNotificacionDTO.DiasDespuesSiembra.HasValue)
                    notificacion.DiasDespuesSiembra = updateNotificacionDTO.DiasDespuesSiembra.Value;

                if (updateNotificacionDTO.Mensaje != null)
                    notificacion.Mensaje = updateNotificacionDTO.Mensaje.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificacionAutomaticaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Error al actualizar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // DELETE: api/NotificacionesAutomaticas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotificacionAutomatica(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var notificacion = await _context.NotificacionesAutomaticas.FindAsync(id);

                if (notificacion == null)
                {
                    return NotFound($"Notificación automática con ID {id} no encontrada");
                }

                // Verificar permisos - solo el dueño o admin puede eliminar
                if (currentUserRole != "admin" && notificacion.UsuarioId != currentUserId)
                    return Forbid();

                _context.NotificacionesAutomaticas.Remove(notificacion);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Error al eliminar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/NotificacionesAutomaticas/generar
        [HttpPost("generar")]
        [Authorize(Roles = "admin")] // ← Solo admin puede generar notificaciones automáticas
        public async Task<ActionResult> GenerarNotificacionesAutomaticas()
        {
            try
            {
                var cultivosActivos = await _context.Cultivos
                    .Include(c => c.TipoCultivo)
                    .Include(c => c.Terreno)
                    .ThenInclude(t => t.Agricultor)
                    .ThenInclude(a => a.Usuario)
                    .Where(c => c.Estado == "activo")
                    .ToListAsync();

                var notificacionesGeneradas = new List<string>();

                foreach (var cultivo in cultivosActivos)
                {
                    var diasDesdeSiembra = (DateTime.Today - cultivo.FechaSiembra).Days;

                    // Buscar notificaciones automáticas para este tipo de cultivo
                    var notificacionesConfig = await _context.NotificacionesAutomaticas
                        .Where(na => na.TipoCultivoId == cultivo.TipoCultivoId && na.DiasDespuesSiembra == diasDesdeSiembra)
                        .ToListAsync();

                    foreach (var config in notificacionesConfig)
                    {
                        // Crear notificación para el usuario
                        var notificacion = new Notificacion
                        {
                            UsuarioId = config.UsuarioId,
                            CultivoId = cultivo.Id,
                            Tipo = config.TipoEvento,
                            Mensaje = config.Mensaje.Replace("{cultivo}", cultivo.TipoCultivo.Nombre)
                                                   .Replace("{dias}", diasDesdeSiembra.ToString())
                                                   .Replace("{terreno}", cultivo.Terreno.Nombre),
                            FechaProgramada = DateTime.Today,
                            Estado = "pendiente"
                        };

                        _context.Notificaciones.Add(notificacion);
                        notificacionesGeneradas.Add($"Notificación para {cultivo.TipoCultivo.Nombre} - {config.TipoEvento}");
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    Mensaje = "Notificaciones automáticas generadas correctamente",
                    Count = notificacionesGeneradas.Count,
                    Notificaciones = notificacionesGeneradas 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar notificaciones automáticas: {ex.Message}");
            }
        }

        private bool NotificacionAutomaticaExists(int id)
        {
            return _context.NotificacionesAutomaticas.Any(e => e.Id == id);
        }

        private bool IsValidTipoEvento(string tipoEvento)
        {
            return tipoEvento == "riego" || tipoEvento == "fumigacion" || 
                   tipoEvento == "cosecha" || tipoEvento == "mantenimiento";
        }
    }
}