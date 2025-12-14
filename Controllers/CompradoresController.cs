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
    public class CompradoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompradoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Compradores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompradorDTO>>> GetCompradores()
        {
            try
            {
                var compradores = await _context.Compradores
                    .Include(c => c.Ventas)
                    .Select(c => new CompradorDTO
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Ruc = c.Ruc,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Contacto = c.Contacto,
                        Email = c.Email,
                        TipoComprador = c.TipoComprador,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TotalCompras = c.Ventas.Count,
                        TotalComprado = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg),
                        UltimaCompra = c.Ventas.Any() ? c.Ventas.Max(v => v.Fecha) : null
                    })
                    .ToListAsync();

                return Ok(compradores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Compradores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompradorDTO>> GetComprador(int id)
        {
            try
            {
                var comprador = await _context.Compradores
                    .Include(c => c.Ventas)
                    .Where(c => c.Id == id)
                    .Select(c => new CompradorDTO
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Ruc = c.Ruc,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Contacto = c.Contacto,
                        Email = c.Email,
                        TipoComprador = c.TipoComprador,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TotalCompras = c.Ventas.Count,
                        TotalComprado = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg),
                        UltimaCompra = c.Ventas.Any() ? c.Ventas.Max(v => v.Fecha) : null
                    })
                    .FirstOrDefaultAsync();

                if (comprador == null)
                {
                    return NotFound($"Comprador con ID {id} no encontrado");
                }

                return comprador;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Compradores/tipo/mayorista
        [HttpGet("tipo/{tipoComprador}")]
        public async Task<ActionResult<IEnumerable<CompradorDTO>>> GetCompradoresByTipo(string tipoComprador)
        {
            try
            {
                var compradores = await _context.Compradores
                    .Include(c => c.Ventas)
                    .Where(c => c.TipoComprador != null && c.TipoComprador.ToLower() == tipoComprador.ToLower())
                    .Select(c => new CompradorDTO
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Ruc = c.Ruc,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Contacto = c.Contacto,
                        Email = c.Email,
                        TipoComprador = c.TipoComprador,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        TotalCompras = c.Ventas.Count,
                        TotalComprado = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg),
                        UltimaCompra = c.Ventas.Any() ? c.Ventas.Max(v => v.Fecha) : null
                    })
                    .ToListAsync();

                return Ok(compradores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Compradores
        [HttpPost]
        [Authorize(Roles = "admin")] // ← Solo admin puede crear
        public async Task<ActionResult<CompradorDTO>> PostComprador(CreateCompradorDTO createCompradorDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createCompradorDTO.Nombre))
                {
                    return BadRequest("Nombre es un campo requerido");
                }

                // Validar tipo de comprador si se proporciona
                if (!string.IsNullOrWhiteSpace(createCompradorDTO.TipoComprador) && 
                    !IsValidTipoComprador(createCompradorDTO.TipoComprador))
                {
                    return BadRequest("Tipo de comprador no válido. Los valores permitidos son: mayorista, minorista, industria, exportador");
                }

                // Verificar si el RUC ya existe (si se proporciona)
                if (!string.IsNullOrWhiteSpace(createCompradorDTO.Ruc))
                {
                    if (await _context.Compradores.AnyAsync(c => c.Ruc == createCompradorDTO.Ruc))
                    {
                        return BadRequest("El RUC ya está registrado por otro comprador");
                    }
                }

                var comprador = new Comprador
                {
                    Nombre = createCompradorDTO.Nombre.Trim(),
                    Ruc = createCompradorDTO.Ruc?.Trim(),
                    Telefono = createCompradorDTO.Telefono?.Trim(),
                    Direccion = createCompradorDTO.Direccion?.Trim(),
                    Contacto = createCompradorDTO.Contacto?.Trim(),
                    Email = createCompradorDTO.Email?.Trim(),
                    TipoComprador = createCompradorDTO.TipoComprador?.Trim().ToLower()
                };

                _context.Compradores.Add(comprador);
                await _context.SaveChangesAsync();

                var compradorDTO = new CompradorDTO
                {
                    Id = comprador.Id,
                    Nombre = comprador.Nombre,
                    Ruc = comprador.Ruc,
                    Telefono = comprador.Telefono,
                    Direccion = comprador.Direccion,
                    Contacto = comprador.Contacto,
                    Email = comprador.Email,
                    TipoComprador = comprador.TipoComprador,
                    CreatedAt = comprador.CreatedAt,
                    UpdatedAt = comprador.UpdatedAt,
                    TotalCompras = 0,
                    TotalComprado = 0,
                    UltimaCompra = null
                };

                return CreatedAtAction(nameof(GetComprador), new { id = comprador.Id }, compradorDTO);
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

        // PUT: api/Compradores/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede actualizar
        public async Task<IActionResult> PutComprador(int id, UpdateCompradorDTO updateCompradorDTO)
        {
            try
            {
                var comprador = await _context.Compradores.FindAsync(id);
                if (comprador == null)
                {
                    return NotFound($"Comprador con ID {id} no encontrado");
                }

                // Validar tipo de comprador si se está actualizando
                if (updateCompradorDTO.TipoComprador != null && !IsValidTipoComprador(updateCompradorDTO.TipoComprador))
                {
                    return BadRequest("Tipo de comprador no válido. Los valores permitidos son: mayorista, minorista, industria, exportador");
                }

                // Verificar si el RUC ya existe (si se está actualizando)
                if (updateCompradorDTO.Ruc != null && updateCompradorDTO.Ruc != comprador.Ruc)
                {
                    if (await _context.Compradores.AnyAsync(c => c.Ruc == updateCompradorDTO.Ruc && c.Id != id))
                    {
                        return BadRequest("El RUC ya está registrado por otro comprador");
                    }
                    comprador.Ruc = updateCompradorDTO.Ruc.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateCompradorDTO.Nombre != null)
                    comprador.Nombre = updateCompradorDTO.Nombre.Trim();

                if (updateCompradorDTO.Telefono != null)
                    comprador.Telefono = updateCompradorDTO.Telefono.Trim();

                if (updateCompradorDTO.Direccion != null)
                    comprador.Direccion = updateCompradorDTO.Direccion.Trim();

                if (updateCompradorDTO.Contacto != null)
                    comprador.Contacto = updateCompradorDTO.Contacto.Trim();

                if (updateCompradorDTO.Email != null)
                    comprador.Email = updateCompradorDTO.Email.Trim();

                if (updateCompradorDTO.TipoComprador != null)
                    comprador.TipoComprador = updateCompradorDTO.TipoComprador.Trim().ToLower();

                comprador.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompradorExists(id))
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

        // DELETE: api/Compradores/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar
        public async Task<IActionResult> DeleteComprador(int id)
        {
            try
            {
                var comprador = await _context.Compradores
                    .Include(c => c.Ventas)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (comprador == null)
                {
                    return NotFound($"Comprador con ID {id} no encontrado");
                }

                // Verificar si hay ventas asociadas
                if (comprador.Ventas.Any())
                {
                    return BadRequest("No se puede eliminar el comprador porque tiene ventas asociadas. Elimine primero las ventas.");
                }

                _context.Compradores.Remove(comprador);
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

        private bool CompradorExists(int id)
        {
            return _context.Compradores.Any(e => e.Id == id);
        }

        private bool IsValidTipoComprador(string tipoComprador)
        {
            var tiposValidos = new[] 
            { 
                "mayorista", "minorista", "industria", "exportador" 
            };
            return tiposValidos.Contains(tipoComprador.ToLower());
        }
    }
}