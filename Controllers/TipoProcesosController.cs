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
    [Authorize] // ← Proteger por defecto, pero GET puede ser público
    public class TipoProcesosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoProcesosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoProcesos
        [HttpGet]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<IEnumerable<TipoProcesoDTO>>> GetTipoProcesos()
        {
            try
            {
                var tipoProcesos = await _context.TipoProcesos
                    .Select(tp => new TipoProcesoDTO
                    {
                        Id = tp.Id,
                        Nombre = tp.Nombre,
                        Descripcion = tp.Descripcion,
                        CreatedAt = tp.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tipoProcesos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/TipoProcesos/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<TipoProcesoDTO>> GetTipoProceso(int id)
        {
            try
            {
                var tipoProceso = await _context.TipoProcesos
                    .Where(tp => tp.Id == id)
                    .Select(tp => new TipoProcesoDTO
                    {
                        Id = tp.Id,
                        Nombre = tp.Nombre,
                        Descripcion = tp.Descripcion,
                        CreatedAt = tp.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (tipoProceso == null)
                {
                    return NotFound($"Tipo de proceso con ID {id} no encontrado");
                }

                return tipoProceso;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/TipoProcesos
        [HttpPost]
        [Authorize(Roles = "admin")] // ← Solo admin puede crear
        public async Task<ActionResult<TipoProcesoDTO>> PostTipoProceso(CreateTipoProcesoDTO createTipoProcesoDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createTipoProcesoDTO.Nombre))
                {
                    return BadRequest("Nombre es un campo requerido");
                }

                // Verificar si el nombre ya existe
                if (await _context.TipoProcesos.AnyAsync(tp => tp.Nombre == createTipoProcesoDTO.Nombre))
                {
                    return BadRequest("El nombre del tipo de proceso ya existe");
                }

                var tipoProceso = new TipoProceso
                {
                    Nombre = createTipoProcesoDTO.Nombre.Trim(),
                    Descripcion = createTipoProcesoDTO.Descripcion?.Trim()
                };

                _context.TipoProcesos.Add(tipoProceso);
                await _context.SaveChangesAsync();

                var tipoProcesoDTO = new TipoProcesoDTO
                {
                    Id = tipoProceso.Id,
                    Nombre = tipoProceso.Nombre,
                    Descripcion = tipoProceso.Descripcion,
                    CreatedAt = tipoProceso.CreatedAt
                };

                return CreatedAtAction(nameof(GetTipoProceso), new { id = tipoProceso.Id }, tipoProcesoDTO);
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

        // PUT: api/TipoProcesos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede actualizar
        public async Task<IActionResult> PutTipoProceso(int id, UpdateTipoProcesoDTO updateTipoProcesoDTO)
        {
            try
            {
                var tipoProceso = await _context.TipoProcesos.FindAsync(id);
                if (tipoProceso == null)
                {
                    return NotFound($"Tipo de proceso con ID {id} no encontrado");
                }

                // Verificar si el nombre ya existe (si se está actualizando)
                if (updateTipoProcesoDTO.Nombre != null && updateTipoProcesoDTO.Nombre != tipoProceso.Nombre)
                {
                    if (await _context.TipoProcesos.AnyAsync(tp => tp.Nombre == updateTipoProcesoDTO.Nombre && tp.Id != id))
                    {
                        return BadRequest("El nombre del tipo de proceso ya existe");
                    }
                    tipoProceso.Nombre = updateTipoProcesoDTO.Nombre.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateTipoProcesoDTO.Descripcion != null)
                    tipoProceso.Descripcion = updateTipoProcesoDTO.Descripcion.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoProcesoExists(id))
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

        // DELETE: api/TipoProcesos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar
        public async Task<IActionResult> DeleteTipoProceso(int id)
        {
            try
            {
                var tipoProceso = await _context.TipoProcesos.FindAsync(id);
                if (tipoProceso == null)
                {
                    return NotFound($"Tipo de proceso con ID {id} no encontrado");
                }

                // Verificar si hay procesos agrícolas usando este tipo
                if (await _context.ProcesosAgricolas.AnyAsync(p => p.TipoProcesoId == id))
                {
                    return BadRequest("No se puede eliminar el tipo de proceso porque está siendo usado por uno o más procesos agrícolas");
                }

                _context.TipoProcesos.Remove(tipoProceso);
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

        private bool TipoProcesoExists(int id)
        {
            return _context.TipoProcesos.Any(e => e.Id == id);
        }
    }
}