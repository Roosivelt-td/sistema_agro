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
    public class DetallesPreparacionTerrenoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetallesPreparacionTerrenoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DetallesPreparacionTerreno
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetallePreparacionTerrenoDTO>>> GetDetallesPreparacionTerreno()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<DetallePreparacionTerreno> query = _context.DetallesPreparacionTerreno;

                // Si no es admin, solo ver sus detalles
                if (currentUserRole != "admin")
                {
                    query = query.Where(d => d.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var detalles = await query
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Select(d => new DetallePreparacionTerrenoDTO
                    {
                        Id = d.Id,
                        ProcesoId = d.ProcesoId,
                        TipoPreparacion = d.TipoPreparacion,
                        HorasMaquinaria = d.HorasMaquinaria,
                        Costo = d.Costo,
                        Observaciones = d.Observaciones,
                        CreatedAt = d.CreatedAt,
                        ProcesoTipo = d.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = d.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = d.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = d.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(detalles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/DetallesPreparacionTerreno/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetallePreparacionTerrenoDTO>> GetDetallePreparacionTerreno(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var detalle = await _context.DetallesPreparacionTerreno
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(d => d.Id == id)
                    .Select(d => new DetallePreparacionTerrenoDTO
                    {
                        Id = d.Id,
                        ProcesoId = d.ProcesoId,
                        TipoPreparacion = d.TipoPreparacion,
                        HorasMaquinaria = d.HorasMaquinaria,
                        Costo = d.Costo,
                        Observaciones = d.Observaciones,
                        CreatedAt = d.CreatedAt,
                        ProcesoTipo = d.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = d.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = d.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = d.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (detalle == null)
                {
                    return NotFound($"Detalle de preparación de terreno con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.ProcesosAgricolas
                        .Where(pa => pa.Id == detalle.ProcesoId)
                        .Select(pa => pa.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                return detalle;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/DetallesPreparacionTerreno/proceso/5
        [HttpGet("proceso/{procesoId}")]
        public async Task<ActionResult<IEnumerable<DetallePreparacionTerrenoDTO>>> GetDetallesByProceso(int procesoId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos del proceso
                if (currentUserRole != "admin")
                {
                    var procesoUsuarioId = await _context.ProcesosAgricolas
                        .Where(pa => pa.Id == procesoId)
                        .Select(pa => pa.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (procesoUsuarioId != currentUserId)
                        return Forbid();
                }

                var detalles = await _context.DetallesPreparacionTerreno
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(d => d.ProcesoId == procesoId)
                    .Select(d => new DetallePreparacionTerrenoDTO
                    {
                        Id = d.Id,
                        ProcesoId = d.ProcesoId,
                        TipoPreparacion = d.TipoPreparacion,
                        HorasMaquinaria = d.HorasMaquinaria,
                        Costo = d.Costo,
                        Observaciones = d.Observaciones,
                        CreatedAt = d.CreatedAt,
                        ProcesoTipo = d.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = d.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = d.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = d.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(detalles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/DetallesPreparacionTerreno
        [HttpPost]
        public async Task<ActionResult<DetallePreparacionTerrenoDTO>> PostDetallePreparacionTerreno(CreateDetallePreparacionTerrenoDTO createDetalleDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos del DTO
                if (createDetalleDTO == null)
                {
                    return BadRequest("Los datos de entrada no pueden ser nulos");
                }

                if (string.IsNullOrWhiteSpace(createDetalleDTO.TipoPreparacion))
                {
                    return BadRequest("TipoPreparacion es un campo requerido");
                }

                // Verificar si el proceso existe
                var proceso = await _context.ProcesosAgricolas
                    .Include(pa => pa.TipoProceso)
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .FirstOrDefaultAsync(pa => pa.Id == createDetalleDTO.ProcesoId);
                
                if (proceso == null)
                {
                    return BadRequest($"El proceso con ID {createDetalleDTO.ProcesoId} no existe");
                }

                // Verificar permisos del proceso
                if (currentUserRole != "admin" && proceso.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validaciones de null para las relaciones
                if (proceso.TipoProceso == null)
                {
                    return BadRequest("El tipo de proceso no está configurado correctamente");
                }

                if (proceso.Cultivo == null)
                {
                    return BadRequest("El cultivo no está configurado correctamente");
                }

                if (proceso.Cultivo.TipoCultivo == null)
                {
                    return BadRequest("El tipo de cultivo no está configurado correctamente");
                }

                if (proceso.Cultivo.Terreno == null)
                {
                    return BadRequest("El terreno no está configurado correctamente");
                }

                if (proceso.Cultivo.Terreno.Agricultor == null)
                {
                    return BadRequest("El agricultor no está configurado correctamente");
                }

                if (proceso.Cultivo.Terreno.Agricultor.Usuario == null)
                {
                    return BadRequest("El usuario del agricultor no está configurado correctamente");
                }

                // Validación del tipo de proceso - VERSIÓN SIMPLIFICADA
                var nombreTipoProceso = proceso.TipoProceso.Nombre?.ToLower().Trim() ?? "";
                
                // Lista más amplia de tipos válidos
                var tiposPreparacionValidos = new HashSet<string> 
                { 
                    "preparación terreno", 
                    "preparacion terreno",
                    "preparación del terreno", 
                    "preparacion del terreno",
                    "preparar terreno",
                    "preparación de terreno", 
                    "preparacion de terreno",
                    "preparacion terreno",
                    "preparación terreno", // exactamente como está en tu base de datos
                    "preparacion" // incluso más flexible
                };

                if (!tiposPreparacionValidos.Contains(nombreTipoProceso))
                {
                    // Para debugging, muestra exactamente qué está comparando
                    return BadRequest($"Tipo de proceso no válido. Esperado: 'Preparación terreno'. Recibido: '{proceso.TipoProceso.Nombre}' (normalizado: '{nombreTipoProceso}'). Proceso ID: {proceso.Id}, TipoProceso ID: {proceso.TipoProcesoId}");
                }

                // Crear el detalle
                var detalle = new DetallePreparacionTerreno
                {
                    ProcesoId = createDetalleDTO.ProcesoId,
                    TipoPreparacion = createDetalleDTO.TipoPreparacion.Trim(),
                    HorasMaquinaria = createDetalleDTO.HorasMaquinaria,
                    Costo = createDetalleDTO.Costo,
                    Observaciones = createDetalleDTO.Observaciones?.Trim()
                };

                _context.DetallesPreparacionTerreno.Add(detalle);
                await _context.SaveChangesAsync();

                // Crear el DTO de respuesta
                var detalleDTO = new DetallePreparacionTerrenoDTO
                {
                    Id = detalle.Id,
                    ProcesoId = detalle.ProcesoId,
                    TipoPreparacion = detalle.TipoPreparacion,
                    HorasMaquinaria = detalle.HorasMaquinaria,
                    Costo = detalle.Costo,
                    Observaciones = detalle.Observaciones,
                    CreatedAt = detalle.CreatedAt,
                    ProcesoTipo = proceso.TipoProceso.Nombre,
                    CultivoNombre = proceso.Cultivo.TipoCultivo?.Nombre ?? "N/A",
                    TerrenoNombre = proceso.Cultivo.Terreno?.Nombre ?? "N/A",
                    AgricultorNombre = proceso.Cultivo.Terreno?.Agricultor?.Usuario?.Nombre ?? "N/A"
                };

                return CreatedAtAction(nameof(GetDetallePreparacionTerreno), new { id = detalle.Id }, detalleDTO);
            }
            catch (DbUpdateException dbEx)
            {
                // Log más detallado del error
                Console.WriteLine($"Error de base de datos: {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                return StatusCode(500, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Log completo del error
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: api/DetallesPreparacionTerreno/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetallePreparacionTerreno(int id, UpdateDetallePreparacionTerrenoDTO updateDetalleDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var detalle = await _context.DetallesPreparacionTerreno
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(d => d.Id == id);
                
                if (detalle == null)
                {
                    return NotFound($"Detalle de preparación de terreno con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && detalle.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Actualizar solo los campos que se proporcionaron
                if (updateDetalleDTO.TipoPreparacion != null)
                    detalle.TipoPreparacion = updateDetalleDTO.TipoPreparacion.Trim();

                if (updateDetalleDTO.HorasMaquinaria.HasValue)
                    detalle.HorasMaquinaria = updateDetalleDTO.HorasMaquinaria.Value;

                if (updateDetalleDTO.Costo.HasValue)
                    detalle.Costo = updateDetalleDTO.Costo.Value;

                if (updateDetalleDTO.Observaciones != null)
                    detalle.Observaciones = updateDetalleDTO.Observaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetallePreparacionTerrenoExists(id))
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

        // DELETE: api/DetallesPreparacionTerreno/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetallePreparacionTerreno(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var detalle = await _context.DetallesPreparacionTerreno
                    .Include(d => d.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(d => d.Id == id);
                
                if (detalle == null)
                {
                    return NotFound($"Detalle de preparación de terreno con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && detalle.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                _context.DetallesPreparacionTerreno.Remove(detalle);
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

        private bool DetallePreparacionTerrenoExists(int id)
        {
            return _context.DetallesPreparacionTerreno.Any(e => e.Id == id);
        }
    }
}