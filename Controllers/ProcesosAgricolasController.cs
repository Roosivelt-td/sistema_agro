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
    public class ProcesosAgricolasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProcesosAgricolasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ProcesosAgricolas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcesoAgricolaDTO>>> GetProcesosAgricolas()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<ProcesoAgricola> query = _context.ProcesosAgricolas;

                // Si no es admin, solo ver sus procesos
                if (currentUserRole != "admin")
                {
                    query = query.Where(pa => pa.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var procesos = await query
                    .Include(pa => pa.TipoProceso)
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Select(pa => new ProcesoAgricolaDTO
                    {
                        Id = pa.Id,
                        CultivoId = pa.CultivoId,
                        TipoProcesoId = pa.TipoProcesoId,
                        Fecha = pa.Fecha,
                        CostoManoObra = pa.CostoManoObra,
                        Observaciones = pa.Observaciones,
                        CreatedAt = pa.CreatedAt,
                        TipoProcesoNombre = pa.TipoProceso.Nombre,
                        CultivoNombre = pa.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = pa.Cultivo.Terreno.Nombre,
                        AgricultorNombre = pa.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Calcular totales para cada proceso
                foreach (var proceso in procesos)
                {
                    proceso.TotalInsumos = await CalcularTotalInsumos(proceso.Id);
                    proceso.TotalManoObra = await CalcularTotalManoObra(proceso.Id);
                    proceso.TotalProceso = proceso.CostoManoObra + proceso.TotalInsumos + proceso.TotalManoObra;
                }

                return Ok(procesos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/ProcesosAgricolas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProcesoAgricolaDTO>> GetProcesoAgricola(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var proceso = await _context.ProcesosAgricolas
                    .Include(pa => pa.TipoProceso)
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Where(pa => pa.Id == id)
                    .Select(pa => new ProcesoAgricolaDTO
                    {
                        Id = pa.Id,
                        CultivoId = pa.CultivoId,
                        TipoProcesoId = pa.TipoProcesoId,
                        Fecha = pa.Fecha,
                        CostoManoObra = pa.CostoManoObra,
                        Observaciones = pa.Observaciones,
                        CreatedAt = pa.CreatedAt,
                        TipoProcesoNombre = pa.TipoProceso.Nombre,
                        CultivoNombre = pa.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = pa.Cultivo.Terreno.Nombre,
                        AgricultorNombre = pa.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (proceso == null)
                {
                    return NotFound($"Proceso agrícola con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.Cultivos
                        .Where(c => c.Id == proceso.CultivoId)
                        .Select(c => c.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                // Calcular totales
                proceso.TotalInsumos = await CalcularTotalInsumos(proceso.Id);
                proceso.TotalManoObra = await CalcularTotalManoObra(proceso.Id);
                proceso.TotalProceso = proceso.CostoManoObra + proceso.TotalInsumos + proceso.TotalManoObra;

                return proceso;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/ProcesosAgricolas/cultivo/5
        [HttpGet("cultivo/{cultivoId}")]
        public async Task<ActionResult<IEnumerable<ProcesoAgricolaDTO>>> GetProcesosByCultivo(int cultivoId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos del cultivo
                if (currentUserRole != "admin")
                {
                    var cultivoUsuarioId = await _context.Cultivos
                        .Where(c => c.Id == cultivoId)
                        .Select(c => c.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (cultivoUsuarioId != currentUserId)
                        return Forbid();
                }

                var procesos = await _context.ProcesosAgricolas
                    .Include(pa => pa.TipoProceso)
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Where(pa => pa.CultivoId == cultivoId)
                    .Select(pa => new ProcesoAgricolaDTO
                    {
                        Id = pa.Id,
                        CultivoId = pa.CultivoId,
                        TipoProcesoId = pa.TipoProcesoId,
                        Fecha = pa.Fecha,
                        CostoManoObra = pa.CostoManoObra,
                        Observaciones = pa.Observaciones,
                        CreatedAt = pa.CreatedAt,
                        TipoProcesoNombre = pa.TipoProceso.Nombre,
                        CultivoNombre = pa.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = pa.Cultivo.Terreno.Nombre,
                        AgricultorNombre = pa.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                // Calcular totales para cada proceso
                foreach (var proceso in procesos)
                {
                    proceso.TotalInsumos = await CalcularTotalInsumos(proceso.Id);
                    proceso.TotalManoObra = await CalcularTotalManoObra(proceso.Id);
                    proceso.TotalProceso = proceso.CostoManoObra + proceso.TotalInsumos + proceso.TotalManoObra;
                }

                return Ok(procesos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/ProcesosAgricolas
        [HttpPost]
        public async Task<ActionResult<ProcesoAgricolaDTO>> PostProcesoAgricola(CreateProcesoAgricolaDTO createProcesoAgricolaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createProcesoAgricolaDTO.Fecha == default)
                {
                    return BadRequest("Fecha es un campo requerido");
                }

                // Verificar si el cultivo existe
                var cultivo = await _context.Cultivos
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .Include(c => c.TipoCultivo)
                    .FirstOrDefaultAsync(c => c.Id == createProcesoAgricolaDTO.CultivoId);
                
                if (cultivo == null)
                {
                    return BadRequest("El cultivo especificado no existe");
                }

                // Verificar permisos del cultivo
                if (currentUserRole != "admin" && cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si el tipo de proceso existe
                var tipoProceso = await _context.TipoProcesos
                    .FirstOrDefaultAsync(tp => tp.Id == createProcesoAgricolaDTO.TipoProcesoId);
                
                if (tipoProceso == null)
                {
                    return BadRequest("El tipo de proceso especificado no existe");
                }

                // Validar que la fecha del proceso esté dentro del rango del cultivo
                if (createProcesoAgricolaDTO.Fecha < cultivo.FechaSiembra)
                {
                    return BadRequest("La fecha del proceso no puede ser anterior a la fecha de siembra del cultivo");
                }

                var proceso = new ProcesoAgricola
                {
                    CultivoId = createProcesoAgricolaDTO.CultivoId,
                    TipoProcesoId = createProcesoAgricolaDTO.TipoProcesoId,
                    Fecha = createProcesoAgricolaDTO.Fecha.Date,
                    CostoManoObra = createProcesoAgricolaDTO.CostoManoObra,
                    Observaciones = createProcesoAgricolaDTO.Observaciones?.Trim()
                };

                _context.ProcesosAgricolas.Add(proceso);
                await _context.SaveChangesAsync();

                var procesoDTO = new ProcesoAgricolaDTO
                {
                    Id = proceso.Id,
                    CultivoId = proceso.CultivoId,
                    TipoProcesoId = proceso.TipoProcesoId,
                    Fecha = proceso.Fecha,
                    CostoManoObra = proceso.CostoManoObra,
                    Observaciones = proceso.Observaciones,
                    CreatedAt = proceso.CreatedAt,
                    TipoProcesoNombre = tipoProceso.Nombre,
                    CultivoNombre = cultivo.TipoCultivo.Nombre,
                    TerrenoNombre = cultivo.Terreno.Nombre,
                    AgricultorNombre = cultivo.Terreno.Agricultor.Usuario.Nombre,
                    TotalInsumos = 0,
                    TotalManoObra = 0,
                    TotalProceso = proceso.CostoManoObra
                };

                return CreatedAtAction(nameof(GetProcesoAgricola), new { id = proceso.Id }, procesoDTO);
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

        // PUT: api/ProcesosAgricolas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProcesoAgricola(int id, UpdateProcesoAgricolaDTO updateProcesoAgricolaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var proceso = await _context.ProcesosAgricolas
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(pa => pa.Id == id);
                
                if (proceso == null)
                {
                    return NotFound($"Proceso agrícola con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && proceso.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validar fecha si se está actualizando
                if (updateProcesoAgricolaDTO.Fecha.HasValue)
                {
                    if (updateProcesoAgricolaDTO.Fecha.Value < proceso.Cultivo.FechaSiembra)
                    {
                        return BadRequest("La fecha del proceso no puede ser anterior a la fecha de siembra del cultivo");
                    }
                    proceso.Fecha = updateProcesoAgricolaDTO.Fecha.Value.Date;
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateProcesoAgricolaDTO.CostoManoObra.HasValue)
                    proceso.CostoManoObra = updateProcesoAgricolaDTO.CostoManoObra.Value;

                if (updateProcesoAgricolaDTO.Observaciones != null)
                    proceso.Observaciones = updateProcesoAgricolaDTO.Observaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProcesoAgricolaExists(id))
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

        // DELETE: api/ProcesosAgricolas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcesoAgricola(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var proceso = await _context.ProcesosAgricolas
                    .Include(pa => pa.Cultivo)
                        .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(pa => pa.Id == id);
                
                if (proceso == null)
                {
                    return NotFound($"Proceso agrícola con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && proceso.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si hay detalles de preparación de terreno asociados
                if (await _context.DetallesPreparacionTerreno.AnyAsync(d => d.ProcesoId == id))
                {
                    return BadRequest("No se puede eliminar el proceso porque tiene detalles de preparación de terreno asociados");
                }

                // Verificar si hay insumos utilizados asociados
                if (await _context.InsumosUtilizados.AnyAsync(i => i.ProcesoId == id))
                {
                    return BadRequest("No se puede eliminar el proceso porque tiene insumos utilizados asociados");
                }

                // Verificar si hay mano de obra asociada
                if (await _context.ManosObra.AnyAsync(m => m.ProcesoId == id))
                {
                    return BadRequest("No se puede eliminar el proceso porque tiene mano de obra asociada");
                }

                _context.ProcesosAgricolas.Remove(proceso);
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

        private bool ProcesoAgricolaExists(int id)
        {
            return _context.ProcesosAgricolas.Any(e => e.Id == id);
        }

        private async Task<decimal> CalcularTotalInsumos(int procesoId)
        {
            return await _context.InsumosUtilizados
                .Where(i => i.ProcesoId == procesoId)
                .SumAsync(i => (i.Cantidad * i.CostoUnitario) + i.CostoFlete);
        }

        private async Task<decimal> CalcularTotalManoObra(int procesoId)
        {
            return await _context.ManosObra
                .Where(m => m.ProcesoId == procesoId)
                .SumAsync(m => m.CostoTotal);
        }
    }
}