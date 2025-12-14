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
    public class TipoInsumosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoInsumosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TipoInsumos
        [HttpGet]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<IEnumerable<TipoInsumoDTO>>> GetTipoInsumos()
        {
            try
            {
                var tipoInsumos = await _context.TipoInsumos
                    .Select(ti => new TipoInsumoDTO
                    {
                        Id = ti.Id,
                        Nombre = ti.Nombre,
                        Categoria = ti.Categoria,
                        Descripcion = ti.Descripcion,
                        CreatedAt = ti.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tipoInsumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/TipoInsumos/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<TipoInsumoDTO>> GetTipoInsumo(int id)
        {
            try
            {
                var tipoInsumo = await _context.TipoInsumos
                    .Where(ti => ti.Id == id)
                    .Select(ti => new TipoInsumoDTO
                    {
                        Id = ti.Id,
                        Nombre = ti.Nombre,
                        Categoria = ti.Categoria,
                        Descripcion = ti.Descripcion,
                        CreatedAt = ti.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (tipoInsumo == null)
                {
                    return NotFound($"Tipo de insumo con ID {id} no encontrado");
                }

                return tipoInsumo;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/TipoInsumos/categoria/semilla
        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous] // ← Hacer público si se desea acceso sin autenticación
        public async Task<ActionResult<IEnumerable<TipoInsumoDTO>>> GetTipoInsumosByCategoria(string categoria)
        {
            try
            {
                var tipoInsumos = await _context.TipoInsumos
                    .Where(ti => ti.Categoria.ToLower() == categoria.ToLower())
                    .Select(ti => new TipoInsumoDTO
                    {
                        Id = ti.Id,
                        Nombre = ti.Nombre,
                        Categoria = ti.Categoria,
                        Descripcion = ti.Descripcion,
                        CreatedAt = ti.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tipoInsumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/TipoInsumos
        [HttpPost]
        [Authorize(Roles = "admin")] // ← Solo admin puede crear
        public async Task<ActionResult<TipoInsumoDTO>> PostTipoInsumo(CreateTipoInsumoDTO createTipoInsumoDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createTipoInsumoDTO.Nombre))
                {
                    return BadRequest("Nombre es un campo requerido");
                }

                if (string.IsNullOrWhiteSpace(createTipoInsumoDTO.Categoria))
                {
                    return BadRequest("Categoria es un campo requerido");
                }

                // Validar categoría
                if (!IsValidCategoria(createTipoInsumoDTO.Categoria))
                {
                    return BadRequest("Categoría no válida. Las categorías permitidas son: semilla, fertilizante, herbicida, insecticida, comida_peones, empaque, otros");
                }

                // Verificar si el nombre ya existe
                if (await _context.TipoInsumos.AnyAsync(ti => ti.Nombre == createTipoInsumoDTO.Nombre))
                {
                    return BadRequest("El nombre del tipo de insumo ya existe");
                }

                var tipoInsumo = new TipoInsumo
                {
                    Nombre = createTipoInsumoDTO.Nombre.Trim(),
                    Categoria = createTipoInsumoDTO.Categoria.Trim().ToLower(),
                    Descripcion = createTipoInsumoDTO.Descripcion?.Trim()
                };

                _context.TipoInsumos.Add(tipoInsumo);
                await _context.SaveChangesAsync();

                var tipoInsumoDTO = new TipoInsumoDTO
                {
                    Id = tipoInsumo.Id,
                    Nombre = tipoInsumo.Nombre,
                    Categoria = tipoInsumo.Categoria,
                    Descripcion = tipoInsumo.Descripcion,
                    CreatedAt = tipoInsumo.CreatedAt
                };

                return CreatedAtAction(nameof(GetTipoInsumo), new { id = tipoInsumo.Id }, tipoInsumoDTO);
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

        // PUT: api/TipoInsumos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede actualizar
        public async Task<IActionResult> PutTipoInsumo(int id, UpdateTipoInsumoDTO updateTipoInsumoDTO)
        {
            try
            {
                var tipoInsumo = await _context.TipoInsumos.FindAsync(id);
                if (tipoInsumo == null)
                {
                    return NotFound($"Tipo de insumo con ID {id} no encontrado");
                }

                // Validar categoría si se está actualizando
                if (updateTipoInsumoDTO.Categoria != null && !IsValidCategoria(updateTipoInsumoDTO.Categoria))
                {
                    return BadRequest("Categoría no válida. Las categorías permitidas son: semilla, fertilizante, herbicida, insecticida, comida_peones, empaque, otros");
                }

                // Verificar si el nombre ya existe (si se está actualizando)
                if (updateTipoInsumoDTO.Nombre != null && updateTipoInsumoDTO.Nombre != tipoInsumo.Nombre)
                {
                    if (await _context.TipoInsumos.AnyAsync(ti => ti.Nombre == updateTipoInsumoDTO.Nombre && ti.Id != id))
                    {
                        return BadRequest("El nombre del tipo de insumo ya existe");
                    }
                    tipoInsumo.Nombre = updateTipoInsumoDTO.Nombre.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateTipoInsumoDTO.Categoria != null)
                    tipoInsumo.Categoria = updateTipoInsumoDTO.Categoria.Trim().ToLower();

                if (updateTipoInsumoDTO.Descripcion != null)
                    tipoInsumo.Descripcion = updateTipoInsumoDTO.Descripcion.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoInsumoExists(id))
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

        // DELETE: api/TipoInsumos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar
        public async Task<IActionResult> DeleteTipoInsumo(int id)
        {
            try
            {
                var tipoInsumo = await _context.TipoInsumos.FindAsync(id);
                if (tipoInsumo == null)
                {
                    return NotFound($"Tipo de insumo con ID {id} no encontrado");
                }

                // Verificar si hay insumos utilizados usando este tipo
                if (await _context.InsumosUtilizados.AnyAsync(i => i.TipoInsumoId == id))
                {
                    return BadRequest("No se puede eliminar el tipo de insumo porque está siendo usado por uno o más insumos utilizados");
                }

                _context.TipoInsumos.Remove(tipoInsumo);
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

        private bool TipoInsumoExists(int id)
        {
            return _context.TipoInsumos.Any(e => e.Id == id);
        }

        private bool IsValidCategoria(string categoria)
        {
            var categoriasValidas = new[] 
            { 
                "semilla", "fertilizante", "herbicida", "insecticida", 
                "comida_peones", "empaque", "otros" 
            };
            return categoriasValidas.Contains(categoria.ToLower());
        }
    }
}