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
    public class TipoCultivosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoCultivosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoCultivos
        [HttpGet]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<IEnumerable<TipoCultivoDTO>>> GetTipoCultivos()
        {
            try
            {
                var tipoCultivos = await _context.TipoCultivos
                    .Select(tc => new TipoCultivoDTO
                    {
                        Id = tc.Id,
                        Nombre = tc.Nombre,
                        TiempoSiembraCosecha = tc.TiempoSiembraCosecha,
                        InstruccionesRiegos = tc.InstruccionesRiegos,
                        InstruccionesFumigaciones = tc.InstruccionesFumigaciones,
                        CreatedAt = tc.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tipoCultivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/TipoCultivos/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<TipoCultivoDTO>> GetTipoCultivo(int id)
        {
            try
            {
                var tipoCultivo = await _context.TipoCultivos
                    .Where(tc => tc.Id == id)
                    .Select(tc => new TipoCultivoDTO
                    {
                        Id = tc.Id,
                        Nombre = tc.Nombre,
                        TiempoSiembraCosecha = tc.TiempoSiembraCosecha,
                        InstruccionesRiegos = tc.InstruccionesRiegos,
                        InstruccionesFumigaciones = tc.InstruccionesFumigaciones,
                        CreatedAt = tc.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (tipoCultivo == null)
                {
                    return NotFound($"Tipo de cultivo con ID {id} no encontrado");
                }

                return tipoCultivo;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/TipoCultivos
        [HttpPost]
        [Authorize(Roles = "admin")] // ← Solo admin puede crear
        public async Task<ActionResult<TipoCultivoDTO>> PostTipoCultivo(CreateTipoCultivoDTO createTipoCultivoDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createTipoCultivoDTO.Nombre))
                {
                    return BadRequest("Nombre es un campo requerido");
                }

                // Verificar si el nombre ya existe
                if (await _context.TipoCultivos.AnyAsync(tc => tc.Nombre == createTipoCultivoDTO.Nombre))
                {
                    return BadRequest("El nombre del tipo de cultivo ya existe");
                }

                var tipoCultivo = new TipoCultivo
                {
                    Nombre = createTipoCultivoDTO.Nombre.Trim(),
                    TiempoSiembraCosecha = createTipoCultivoDTO.TiempoSiembraCosecha,
                    InstruccionesRiegos = createTipoCultivoDTO.InstruccionesRiegos?.Trim(),
                    InstruccionesFumigaciones = createTipoCultivoDTO.InstruccionesFumigaciones?.Trim()
                };

                _context.TipoCultivos.Add(tipoCultivo);
                await _context.SaveChangesAsync();

                var tipoCultivoDTO = new TipoCultivoDTO
                {
                    Id = tipoCultivo.Id,
                    Nombre = tipoCultivo.Nombre,
                    TiempoSiembraCosecha = tipoCultivo.TiempoSiembraCosecha,
                    InstruccionesRiegos = tipoCultivo.InstruccionesRiegos,
                    InstruccionesFumigaciones = tipoCultivo.InstruccionesFumigaciones,
                    CreatedAt = tipoCultivo.CreatedAt
                };

                return CreatedAtAction(nameof(GetTipoCultivo), new { id = tipoCultivo.Id }, tipoCultivoDTO);
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

        // PUT: api/TipoCultivos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede actualizar
        public async Task<IActionResult> PutTipoCultivo(int id, UpdateTipoCultivoDTO updateTipoCultivoDTO)
        {
            try
            {
                var tipoCultivo = await _context.TipoCultivos.FindAsync(id);
                if (tipoCultivo == null)
                {
                    return NotFound($"Tipo de cultivo con ID {id} no encontrado");
                }

                // Verificar si el nombre ya existe (si se está actualizando)
                if (updateTipoCultivoDTO.Nombre != null && updateTipoCultivoDTO.Nombre != tipoCultivo.Nombre)
                {
                    if (await _context.TipoCultivos.AnyAsync(tc => tc.Nombre == updateTipoCultivoDTO.Nombre && tc.Id != id))
                    {
                        return BadRequest("El nombre del tipo de cultivo ya existe");
                    }
                    tipoCultivo.Nombre = updateTipoCultivoDTO.Nombre.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateTipoCultivoDTO.TiempoSiembraCosecha.HasValue)
                    tipoCultivo.TiempoSiembraCosecha = updateTipoCultivoDTO.TiempoSiembraCosecha.Value;

                if (updateTipoCultivoDTO.InstruccionesRiegos != null)
                    tipoCultivo.InstruccionesRiegos = updateTipoCultivoDTO.InstruccionesRiegos.Trim();

                if (updateTipoCultivoDTO.InstruccionesFumigaciones != null)
                    tipoCultivo.InstruccionesFumigaciones = updateTipoCultivoDTO.InstruccionesFumigaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoCultivoExists(id))
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

        // DELETE: api/TipoCultivos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar
        public async Task<IActionResult> DeleteTipoCultivo(int id)
        {
            try
            {
                var tipoCultivo = await _context.TipoCultivos.FindAsync(id);
                if (tipoCultivo == null)
                {
                    return NotFound($"Tipo de cultivo con ID {id} no encontrado");
                }

                // Verificar si hay cultivos usando este tipo
                if (await _context.Cultivos.AnyAsync(c => c.TipoCultivoId == id))
                {
                    return BadRequest("No se puede eliminar el tipo de cultivo porque está siendo usado por uno o más cultivos");
                }

                _context.TipoCultivos.Remove(tipoCultivo);
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

        private bool TipoCultivoExists(int id)
        {
            return _context.TipoCultivos.Any(e => e.Id == id);
        }
    }
}