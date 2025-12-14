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
    public class CultivosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CultivosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cultivos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CultivoDTO>>> GetCultivos()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Cultivo> query = _context.Cultivos;

                // Si no es admin, solo ver sus cultivos
                if (currentUserRole != "admin")
                {
                    query = query.Where(c => c.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var cultivos = await query
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .Include(c => c.TipoCultivo)
                    .Select(c => new CultivoDTO
                    {
                        Id = c.Id,
                        TerrenoId = c.TerrenoId,
                        TipoCultivoId = c.TipoCultivoId,
                        FechaSiembra = c.FechaSiembra,
                        FechaCosechaEstimada = c.FechaCosechaEstimada,
                        Estado = c.Estado,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TerrenoNombre = c.Terreno.Nombre,
                        TipoCultivoNombre = c.TipoCultivo.Nombre,
                        AgricultorNombre = c.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Calcular propiedades calculadas después de obtener los datos
                foreach (var cultivo in cultivos)
                {
                    cultivo.DiasRestantes = (cultivo.FechaCosechaEstimada - DateTime.Today).Days;
                    cultivo.EstaAtrasado = cultivo.Estado == "activo" && DateTime.Today > cultivo.FechaCosechaEstimada;
                }

                return Ok(cultivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Cultivos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CultivoDTO>> GetCultivo(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cultivo = await _context.Cultivos
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .Include(c => c.TipoCultivo)
                    .Where(c => c.Id == id)
                    .Select(c => new CultivoDTO
                    {
                        Id = c.Id,
                        TerrenoId = c.TerrenoId,
                        TipoCultivoId = c.TipoCultivoId,
                        FechaSiembra = c.FechaSiembra,
                        FechaCosechaEstimada = c.FechaCosechaEstimada,
                        Estado = c.Estado,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TerrenoNombre = c.Terreno.Nombre,
                        TipoCultivoNombre = c.TipoCultivo.Nombre,
                        AgricultorNombre = c.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (cultivo == null)
                {
                    return NotFound($"Cultivo con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.Terrenos
                        .Where(t => t.Id == cultivo.TerrenoId)
                        .Select(t => t.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                // Calcular propiedades calculadas
                cultivo.DiasRestantes = (cultivo.FechaCosechaEstimada - DateTime.Today).Days;
                cultivo.EstaAtrasado = cultivo.Estado == "activo" && DateTime.Today > cultivo.FechaCosechaEstimada;

                return cultivo;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Cultivos/terreno/5
        [HttpGet("terreno/{terrenoId}")]
        public async Task<ActionResult<IEnumerable<CultivoDTO>>> GetCultivosByTerreno(int terrenoId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos del terreno
                if (currentUserRole != "admin")
                {
                    var terrenoUsuarioId = await _context.Terrenos
                        .Where(t => t.Id == terrenoId)
                        .Select(t => t.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (terrenoUsuarioId != currentUserId)
                        return Forbid();
                }

                var cultivos = await _context.Cultivos
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .Include(c => c.TipoCultivo)
                    .Where(c => c.TerrenoId == terrenoId)
                    .Select(c => new CultivoDTO
                    {
                        Id = c.Id,
                        TerrenoId = c.TerrenoId,
                        TipoCultivoId = c.TipoCultivoId,
                        FechaSiembra = c.FechaSiembra,
                        FechaCosechaEstimada = c.FechaCosechaEstimada,
                        Estado = c.Estado,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TerrenoNombre = c.Terreno.Nombre,
                        TipoCultivoNombre = c.TipoCultivo.Nombre,
                        AgricultorNombre = c.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Calcular propiedades calculadas
                foreach (var cultivo in cultivos)
                {
                    cultivo.DiasRestantes = (cultivo.FechaCosechaEstimada - DateTime.Today).Days;
                    cultivo.EstaAtrasado = cultivo.Estado == "activo" && DateTime.Today > cultivo.FechaCosechaEstimada;
                }

                return Ok(cultivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Cultivos/agricultor/5
        [HttpGet("agricultor/{agricultorId}")]
        public async Task<ActionResult<IEnumerable<CultivoDTO>>> GetCultivosByAgricultor(int agricultorId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.Agricultores
                        .Where(a => a.Id == agricultorId)
                        .Select(a => a.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                var cultivos = await _context.Cultivos
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .Include(c => c.TipoCultivo)
                    .Where(c => c.Terreno.AgricultorId == agricultorId)
                    .Select(c => new CultivoDTO
                    {
                        Id = c.Id,
                        TerrenoId = c.TerrenoId,
                        TipoCultivoId = c.TipoCultivoId,
                        FechaSiembra = c.FechaSiembra,
                        FechaCosechaEstimada = c.FechaCosechaEstimada,
                        Estado = c.Estado,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TerrenoNombre = c.Terreno.Nombre,
                        TipoCultivoNombre = c.TipoCultivo.Nombre,
                        AgricultorNombre = c.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Calcular propiedades calculadas
                foreach (var cultivo in cultivos)
                {
                    cultivo.DiasRestantes = (cultivo.FechaCosechaEstimada - DateTime.Today).Days;
                    cultivo.EstaAtrasado = cultivo.Estado == "activo" && DateTime.Today > cultivo.FechaCosechaEstimada;
                }

                return Ok(cultivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Cultivos
        [HttpPost]
        public async Task<ActionResult<CultivoDTO>> PostCultivo(CreateCultivoDTO createCultivoDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createCultivoDTO.FechaSiembra == default)
                {
                    return BadRequest("FechaSiembra es un campo requerido");
                }

                // Validar estado
                if (!IsValidEstado(createCultivoDTO.Estado))
                {
                    return BadRequest("Estado no válido. Los valores permitidos son: planificado, activo, completado, cancelado");
                }

                // Verificar si el terreno existe
                var terreno = await _context.Terrenos
                    .Include(t => t.Agricultor)
                        .ThenInclude(a => a.Usuario)
                    .FirstOrDefaultAsync(t => t.Id == createCultivoDTO.TerrenoId);
                
                if (terreno == null)
                {
                    return BadRequest("El terreno especificado no existe");
                }

                // Verificar permisos del terreno
                if (currentUserRole != "admin" && terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si el tipo de cultivo existe
                var tipoCultivo = await _context.TipoCultivos
                    .FirstOrDefaultAsync(tc => tc.Id == createCultivoDTO.TipoCultivoId);
                
                if (tipoCultivo == null)
                {
                    return BadRequest("El tipo de cultivo especificado no existe");
                }

                // Calcular fecha de cosecha estimada automáticamente
                var fechaCosechaEstimada = createCultivoDTO.FechaSiembra.AddDays(tipoCultivo.TiempoSiembraCosecha);

                var cultivo = new Cultivo
                {
                    TerrenoId = createCultivoDTO.TerrenoId,
                    TipoCultivoId = createCultivoDTO.TipoCultivoId,
                    FechaSiembra = createCultivoDTO.FechaSiembra.Date,
                    FechaCosechaEstimada = fechaCosechaEstimada.Date,
                    Estado = createCultivoDTO.Estado.Trim()
                };

                _context.Cultivos.Add(cultivo);
                await _context.SaveChangesAsync();

                var cultivoDTO = new CultivoDTO
                {
                    Id = cultivo.Id,
                    TerrenoId = cultivo.TerrenoId,
                    TipoCultivoId = cultivo.TipoCultivoId,
                    FechaSiembra = cultivo.FechaSiembra,
                    FechaCosechaEstimada = cultivo.FechaCosechaEstimada,
                    Estado = cultivo.Estado,
                    CreatedAt = cultivo.CreatedAt,
                    UpdatedAt = cultivo.UpdatedAt,
                    TerrenoNombre = terreno.Nombre,
                    TipoCultivoNombre = tipoCultivo.Nombre,
                    AgricultorNombre = terreno.Agricultor.Usuario.Nombre,
                    DiasRestantes = (cultivo.FechaCosechaEstimada - DateTime.Today).Days,
                    EstaAtrasado = cultivo.Estado == "activo" && DateTime.Today > cultivo.FechaCosechaEstimada
                };

                return CreatedAtAction(nameof(GetCultivo), new { id = cultivo.Id }, cultivoDTO);
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

        // PUT: api/Cultivos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCultivo(int id, UpdateCultivoDTO updateCultivoDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cultivo = await _context.Cultivos
                    .Include(c => c.Terreno)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (cultivo == null)
                {
                    return NotFound($"Cultivo con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validar estado si se está actualizando
                if (updateCultivoDTO.Estado != null && !IsValidEstado(updateCultivoDTO.Estado))
                {
                    return BadRequest("Estado no válido. Los valores permitidos son: planificado, activo, completado, cancelado");
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateCultivoDTO.Estado != null)
                    cultivo.Estado = updateCultivoDTO.Estado.Trim();

                cultivo.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CultivoExists(id))
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

        // DELETE: api/Cultivos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCultivo(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cultivo = await _context.Cultivos
                    .Include(c => c.Terreno)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (cultivo == null)
                {
                    return NotFound($"Cultivo con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                _context.Cultivos.Remove(cultivo);
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

        private bool CultivoExists(int id)
        {
            return _context.Cultivos.Any(e => e.Id == id);
        }

        private bool IsValidEstado(string estado)
        {
            return estado == "planificado" || estado == "activo" || estado == "completado" || estado == "cancelado";
        }
    }
}