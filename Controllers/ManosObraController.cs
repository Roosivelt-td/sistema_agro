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
    public class ManosObraController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManosObraController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ManosObra
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ManoObraDTO>>> GetManosObra()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<ManoObra> query = _context.ManosObra;

                // Si no es admin, solo ver su mano de obra
                if (currentUserRole != "admin")
                {
                    query = query.Where(m => m.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var manosObra = await query
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Select(m => new ManoObraDTO
                    {
                        Id = m.Id,
                        ProcesoId = m.ProcesoId,
                        NumeroPeones = m.NumeroPeones,
                        DiasTrabajo = m.DiasTrabajo,
                        CostoPorDia = m.CostoPorDia,
                        CostoTotal = m.CostoTotal,
                        Observaciones = m.Observaciones,
                        CreatedAt = m.CreatedAt,
                        ProcesoTipo = m.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = m.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = m.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = m.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(manosObra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/ManosObra/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ManoObraDTO>> GetManoObra(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var manoObra = await _context.ManosObra
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(m => m.Id == id)
                    .Select(m => new ManoObraDTO
                    {
                        Id = m.Id,
                        ProcesoId = m.ProcesoId,
                        NumeroPeones = m.NumeroPeones,
                        DiasTrabajo = m.DiasTrabajo,
                        CostoPorDia = m.CostoPorDia,
                        CostoTotal = m.CostoTotal,
                        Observaciones = m.Observaciones,
                        CreatedAt = m.CreatedAt,
                        ProcesoTipo = m.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = m.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = m.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = m.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (manoObra == null)
                {
                    return NotFound($"Mano de obra con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.ProcesosAgricolas
                        .Where(pa => pa.Id == manoObra.ProcesoId)
                        .Select(pa => pa.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                return manoObra;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/ManosObra/proceso/5
        [HttpGet("proceso/{procesoId}")]
        public async Task<ActionResult<IEnumerable<ManoObraDTO>>> GetManosObraByProceso(int procesoId)
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

                var manosObra = await _context.ManosObra
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(m => m.ProcesoId == procesoId)
                    .Select(m => new ManoObraDTO
                    {
                        Id = m.Id,
                        ProcesoId = m.ProcesoId,
                        NumeroPeones = m.NumeroPeones,
                        DiasTrabajo = m.DiasTrabajo,
                        CostoPorDia = m.CostoPorDia,
                        CostoTotal = m.CostoTotal,
                        Observaciones = m.Observaciones,
                        CreatedAt = m.CreatedAt,
                        ProcesoTipo = m.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = m.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = m.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = m.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(manosObra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/ManosObra
        [HttpPost]
        public async Task<ActionResult<ManoObraDTO>> PostManoObra(CreateManoObraDTO createManoObraDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createManoObraDTO.NumeroPeones <= 0)
                {
                    return BadRequest("El número de peones debe ser mayor a 0");
                }

                if (createManoObraDTO.DiasTrabajo <= 0)
                {
                    return BadRequest("Los días de trabajo deben ser mayor a 0");
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
                    .FirstOrDefaultAsync(pa => pa.Id == createManoObraDTO.ProcesoId);
                
                if (proceso == null)
                {
                    return BadRequest("El proceso especificado no existe");
                }

                // Verificar permisos del proceso
                if (currentUserRole != "admin" && proceso.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Calcular costo total automáticamente
                var costoTotal = createManoObraDTO.NumeroPeones * createManoObraDTO.DiasTrabajo * createManoObraDTO.CostoPorDia;

                var manoObra = new ManoObra
                {
                    ProcesoId = createManoObraDTO.ProcesoId,
                    NumeroPeones = createManoObraDTO.NumeroPeones,
                    DiasTrabajo = createManoObraDTO.DiasTrabajo,
                    CostoPorDia = createManoObraDTO.CostoPorDia,
                    CostoTotal = costoTotal,
                    Observaciones = createManoObraDTO.Observaciones?.Trim()
                };

                _context.ManosObra.Add(manoObra);
                await _context.SaveChangesAsync();

                var manoObraDTO = new ManoObraDTO
                {
                    Id = manoObra.Id,
                    ProcesoId = manoObra.ProcesoId,
                    NumeroPeones = manoObra.NumeroPeones,
                    DiasTrabajo = manoObra.DiasTrabajo,
                    CostoPorDia = manoObra.CostoPorDia,
                    CostoTotal = manoObra.CostoTotal,
                    Observaciones = manoObra.Observaciones,
                    CreatedAt = manoObra.CreatedAt,
                    ProcesoTipo = proceso.TipoProceso.Nombre,
                    CultivoNombre = proceso.Cultivo.TipoCultivo.Nombre,
                    TerrenoNombre = proceso.Cultivo.Terreno.Nombre,
                    AgricultorNombre = proceso.Cultivo.Terreno.Agricultor.Usuario.Nombre
                };

                return CreatedAtAction(nameof(GetManoObra), new { id = manoObra.Id }, manoObraDTO);
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

        // PUT: api/ManosObra/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutManoObra(int id, UpdateManoObraDTO updateManoObraDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var manoObra = await _context.ManosObra
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (manoObra == null)
                {
                    return NotFound($"Mano de obra con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && manoObra.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Actualizar solo los campos que se proporcionaron
                bool recalcCostoTotal = false;

                if (updateManoObraDTO.NumeroPeones.HasValue)
                {
                    manoObra.NumeroPeones = updateManoObraDTO.NumeroPeones.Value;
                    recalcCostoTotal = true;
                }

                if (updateManoObraDTO.DiasTrabajo.HasValue)
                {
                    manoObra.DiasTrabajo = updateManoObraDTO.DiasTrabajo.Value;
                    recalcCostoTotal = true;
                }

                if (updateManoObraDTO.CostoPorDia.HasValue)
                {
                    manoObra.CostoPorDia = updateManoObraDTO.CostoPorDia.Value;
                    recalcCostoTotal = true;
                }

                if (updateManoObraDTO.Observaciones != null)
                    manoObra.Observaciones = updateManoObraDTO.Observaciones.Trim();

                // Recalcular costo total si alguno de los campos relevantes cambió
                if (recalcCostoTotal)
                {
                    manoObra.CostoTotal = manoObra.NumeroPeones * manoObra.DiasTrabajo * manoObra.CostoPorDia;
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ManoObraExists(id))
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

        // DELETE: api/ManosObra/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManoObra(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var manoObra = await _context.ManosObra
                    .Include(m => m.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (manoObra == null)
                {
                    return NotFound($"Mano de obra con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && manoObra.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                _context.ManosObra.Remove(manoObra);
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

        private bool ManoObraExists(int id)
        {
            return _context.ManosObra.Any(e => e.Id == id);
        }
    }
}