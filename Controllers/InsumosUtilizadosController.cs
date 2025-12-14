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
    public class InsumosUtilizadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InsumosUtilizadosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InsumosUtilizados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsumoUtilizadoDTO>>> GetInsumosUtilizados()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<InsumoUtilizado> query = _context.InsumosUtilizados;

                // Si no es admin, solo ver sus insumos
                if (currentUserRole != "admin")
                {
                    query = query.Where(i => i.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var insumos = await query
                    .Include(i => i.TipoInsumo)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Select(i => new InsumoUtilizadoDTO
                    {
                        Id = i.Id,
                        ProcesoId = i.ProcesoId,
                        TipoInsumoId = i.TipoInsumoId,
                        Cantidad = i.Cantidad,
                        CostoUnitario = i.CostoUnitario,
                        CostoFlete = i.CostoFlete,
                        Observaciones = i.Observaciones,
                        CreatedAt = i.CreatedAt,
                        CostoTotal = (i.Cantidad * i.CostoUnitario) + i.CostoFlete,
                        TipoInsumoNombre = i.TipoInsumo.Nombre,
                        TipoInsumoCategoria = i.TipoInsumo.Categoria,
                        ProcesoTipo = i.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = i.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = i.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = i.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(insumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/InsumosUtilizados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InsumoUtilizadoDTO>> GetInsumoUtilizado(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var insumo = await _context.InsumosUtilizados
                    .Include(i => i.TipoInsumo)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(i => i.Id == id)
                    .Select(i => new InsumoUtilizadoDTO
                    {
                        Id = i.Id,
                        ProcesoId = i.ProcesoId,
                        TipoInsumoId = i.TipoInsumoId,
                        Cantidad = i.Cantidad,
                        CostoUnitario = i.CostoUnitario,
                        CostoFlete = i.CostoFlete,
                        Observaciones = i.Observaciones,
                        CreatedAt = i.CreatedAt,
                        CostoTotal = (i.Cantidad * i.CostoUnitario) + i.CostoFlete,
                        TipoInsumoNombre = i.TipoInsumo.Nombre,
                        TipoInsumoCategoria = i.TipoInsumo.Categoria,
                        ProcesoTipo = i.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = i.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = i.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = i.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (insumo == null)
                {
                    return NotFound($"Insumo utilizado con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.ProcesosAgricolas
                        .Where(pa => pa.Id == insumo.ProcesoId)
                        .Select(pa => pa.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                return insumo;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/InsumosUtilizados/proceso/5
        [HttpGet("proceso/{procesoId}")]
        public async Task<ActionResult<IEnumerable<InsumoUtilizadoDTO>>> GetInsumosByProceso(int procesoId)
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

                var insumos = await _context.InsumosUtilizados
                    .Include(i => i.TipoInsumo)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(i => i.ProcesoId == procesoId)
                    .Select(i => new InsumoUtilizadoDTO
                    {
                        Id = i.Id,
                        ProcesoId = i.ProcesoId,
                        TipoInsumoId = i.TipoInsumoId,
                        Cantidad = i.Cantidad,
                        CostoUnitario = i.CostoUnitario,
                        CostoFlete = i.CostoFlete,
                        Observaciones = i.Observaciones,
                        CreatedAt = i.CreatedAt,
                        CostoTotal = (i.Cantidad * i.CostoUnitario) + i.CostoFlete,
                        TipoInsumoNombre = i.TipoInsumo.Nombre,
                        TipoInsumoCategoria = i.TipoInsumo.Categoria,
                        ProcesoTipo = i.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = i.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = i.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = i.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(insumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/InsumosUtilizados/cultivo/5
        [HttpGet("cultivo/{cultivoId}")]
        public async Task<ActionResult<IEnumerable<InsumoUtilizadoDTO>>> GetInsumosByCultivo(int cultivoId)
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

                var insumos = await _context.InsumosUtilizados
                    .Include(i => i.TipoInsumo)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.TipoProceso)
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Where(i => i.ProcesoAgricola.CultivoId == cultivoId)
                    .Select(i => new InsumoUtilizadoDTO
                    {
                        Id = i.Id,
                        ProcesoId = i.ProcesoId,
                        TipoInsumoId = i.TipoInsumoId,
                        Cantidad = i.Cantidad,
                        CostoUnitario = i.CostoUnitario,
                        CostoFlete = i.CostoFlete,
                        Observaciones = i.Observaciones,
                        CreatedAt = i.CreatedAt,
                        CostoTotal = (i.Cantidad * i.CostoUnitario) + i.CostoFlete,
                        TipoInsumoNombre = i.TipoInsumo.Nombre,
                        TipoInsumoCategoria = i.TipoInsumo.Categoria,
                        ProcesoTipo = i.ProcesoAgricola.TipoProceso.Nombre,
                        CultivoNombre = i.ProcesoAgricola.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = i.ProcesoAgricola.Cultivo.Terreno.Nombre,
                        AgricultorNombre = i.ProcesoAgricola.Cultivo.Terreno.Agricultor.Usuario.Nombre
                    })
                    .ToListAsync();

                return Ok(insumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/InsumosUtilizados
        [HttpPost]
        public async Task<ActionResult<InsumoUtilizadoDTO>> PostInsumoUtilizado(CreateInsumoUtilizadoDTO createInsumoDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createInsumoDTO.Cantidad <= 0)
                {
                    return BadRequest("La cantidad debe ser mayor a 0");
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
                    .FirstOrDefaultAsync(pa => pa.Id == createInsumoDTO.ProcesoId);
                
                if (proceso == null)
                {
                    return BadRequest("El proceso especificado no existe");
                }

                // Verificar permisos del proceso
                if (currentUserRole != "admin" && proceso.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si el tipo de insumo existe
                var tipoInsumo = await _context.TipoInsumos
                    .FirstOrDefaultAsync(ti => ti.Id == createInsumoDTO.TipoInsumoId);
                
                if (tipoInsumo == null)
                {
                    return BadRequest("El tipo de insumo especificado no existe");
                }

                var insumo = new InsumoUtilizado
                {
                    ProcesoId = createInsumoDTO.ProcesoId,
                    TipoInsumoId = createInsumoDTO.TipoInsumoId,
                    Cantidad = createInsumoDTO.Cantidad,
                    CostoUnitario = createInsumoDTO.CostoUnitario,
                    CostoFlete = createInsumoDTO.CostoFlete,
                    Observaciones = createInsumoDTO.Observaciones?.Trim()
                };

                _context.InsumosUtilizados.Add(insumo);
                await _context.SaveChangesAsync();

                var insumoDTO = new InsumoUtilizadoDTO
                {
                    Id = insumo.Id,
                    ProcesoId = insumo.ProcesoId,
                    TipoInsumoId = insumo.TipoInsumoId,
                    Cantidad = insumo.Cantidad,
                    CostoUnitario = insumo.CostoUnitario,
                    CostoFlete = insumo.CostoFlete,
                    Observaciones = insumo.Observaciones,
                    CreatedAt = insumo.CreatedAt,
                    CostoTotal = (insumo.Cantidad * insumo.CostoUnitario) + insumo.CostoFlete,
                    TipoInsumoNombre = tipoInsumo.Nombre,
                    TipoInsumoCategoria = tipoInsumo.Categoria,
                    ProcesoTipo = proceso.TipoProceso.Nombre,
                    CultivoNombre = proceso.Cultivo.TipoCultivo.Nombre,
                    TerrenoNombre = proceso.Cultivo.Terreno.Nombre,
                    AgricultorNombre = proceso.Cultivo.Terreno.Agricultor.Usuario.Nombre
                };

                return CreatedAtAction(nameof(GetInsumoUtilizado), new { id = insumo.Id }, insumoDTO);
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

        // PUT: api/InsumosUtilizados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInsumoUtilizado(int id, UpdateInsumoUtilizadoDTO updateInsumoDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var insumo = await _context.InsumosUtilizados
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(i => i.Id == id);
                
                if (insumo == null)
                {
                    return NotFound($"Insumo utilizado con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && insumo.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Actualizar solo los campos que se proporcionaron
                if (updateInsumoDTO.Cantidad.HasValue)
                    insumo.Cantidad = updateInsumoDTO.Cantidad.Value;

                if (updateInsumoDTO.CostoUnitario.HasValue)
                    insumo.CostoUnitario = updateInsumoDTO.CostoUnitario.Value;

                if (updateInsumoDTO.CostoFlete.HasValue)
                    insumo.CostoFlete = updateInsumoDTO.CostoFlete.Value;

                if (updateInsumoDTO.Observaciones != null)
                    insumo.Observaciones = updateInsumoDTO.Observaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InsumoUtilizadoExists(id))
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

        // DELETE: api/InsumosUtilizados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInsumoUtilizado(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var insumo = await _context.InsumosUtilizados
                    .Include(i => i.ProcesoAgricola)
                        .ThenInclude(pa => pa.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(i => i.Id == id);
                
                if (insumo == null)
                {
                    return NotFound($"Insumo utilizado con ID {id} no encontrado");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && insumo.ProcesoAgricola.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                _context.InsumosUtilizados.Remove(insumo);
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

        private bool InsumoUtilizadoExists(int id)
        {
            return _context.InsumosUtilizados.Any(e => e.Id == id);
        }
    }
}