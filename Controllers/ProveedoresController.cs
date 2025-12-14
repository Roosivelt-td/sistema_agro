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
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDTO>>> GetProveedores()
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Select(p => new ProveedorDTO
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Ruc = p.Ruc,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        TipoServicio = p.TipoServicio,
                        Contacto = p.Contacto,
                        Email = p.Email,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorDTO>> GetProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores
                    .Where(p => p.Id == id)
                    .Select(p => new ProveedorDTO
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Ruc = p.Ruc,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        TipoServicio = p.TipoServicio,
                        Contacto = p.Contacto,
                        Email = p.Email,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (proveedor == null)
                {
                    return NotFound($"Proveedor con ID {id} no encontrado");
                }

                return proveedor;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Proveedores/servicio/insumos
        [HttpGet("servicio/{tipoServicio}")]
        public async Task<ActionResult<IEnumerable<ProveedorDTO>>> GetProveedoresByTipoServicio(string tipoServicio)
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Where(p => p.TipoServicio.ToLower() == tipoServicio.ToLower())
                    .Select(p => new ProveedorDTO
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Ruc = p.Ruc,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        TipoServicio = p.TipoServicio,
                        Contacto = p.Contacto,
                        Email = p.Email,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Proveedores
        [HttpPost]
        [Authorize(Roles = "admin")] // ← Solo admin puede crear
        public async Task<ActionResult<ProveedorDTO>> PostProveedor(CreateProveedorDTO createProveedorDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createProveedorDTO.Nombre))
                {
                    return BadRequest("Nombre es un campo requerido");
                }

                if (string.IsNullOrWhiteSpace(createProveedorDTO.TipoServicio))
                {
                    return BadRequest("TipoServicio es un campo requerido");
                }

                // Validar tipo de servicio
                if (!IsValidTipoServicio(createProveedorDTO.TipoServicio))
                {
                    return BadRequest("Tipo de servicio no válido. Los valores permitidos son: insumos, maquinaria, transporte, otros");
                }

                // Verificar si el RUC ya existe (si se proporciona)
                if (!string.IsNullOrWhiteSpace(createProveedorDTO.Ruc))
                {
                    if (await _context.Proveedores.AnyAsync(p => p.Ruc == createProveedorDTO.Ruc))
                    {
                        return BadRequest("El RUC ya está registrado por otro proveedor");
                    }
                }

                var proveedor = new Proveedor
                {
                    Nombre = createProveedorDTO.Nombre.Trim(),
                    Ruc = createProveedorDTO.Ruc?.Trim(),
                    Telefono = createProveedorDTO.Telefono?.Trim(),
                    Direccion = createProveedorDTO.Direccion?.Trim(),
                    TipoServicio = createProveedorDTO.TipoServicio.Trim().ToLower(),
                    Contacto = createProveedorDTO.Contacto?.Trim(),
                    Email = createProveedorDTO.Email?.Trim()
                };

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                var proveedorDTO = new ProveedorDTO
                {
                    Id = proveedor.Id,
                    Nombre = proveedor.Nombre,
                    Ruc = proveedor.Ruc,
                    Telefono = proveedor.Telefono,
                    Direccion = proveedor.Direccion,
                    TipoServicio = proveedor.TipoServicio,
                    Contacto = proveedor.Contacto,
                    Email = proveedor.Email,
                    CreatedAt = proveedor.CreatedAt,
                    UpdatedAt = proveedor.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedorDTO);
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

        // PUT: api/Proveedores/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede actualizar
        public async Task<IActionResult> PutProveedor(int id, UpdateProveedorDTO updateProveedorDTO)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                {
                    return NotFound($"Proveedor con ID {id} no encontrado");
                }

                // Validar tipo de servicio si se está actualizando
                if (updateProveedorDTO.TipoServicio != null && !IsValidTipoServicio(updateProveedorDTO.TipoServicio))
                {
                    return BadRequest("Tipo de servicio no válido. Los valores permitidos son: insumos, maquinaria, transporte, otros");
                }

                // Verificar si el RUC ya existe (si se está actualizando)
                if (updateProveedorDTO.Ruc != null && updateProveedorDTO.Ruc != proveedor.Ruc)
                {
                    if (await _context.Proveedores.AnyAsync(p => p.Ruc == updateProveedorDTO.Ruc && p.Id != id))
                    {
                        return BadRequest("El RUC ya está registrado por otro proveedor");
                    }
                    proveedor.Ruc = updateProveedorDTO.Ruc.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateProveedorDTO.Nombre != null)
                    proveedor.Nombre = updateProveedorDTO.Nombre.Trim();

                if (updateProveedorDTO.Telefono != null)
                    proveedor.Telefono = updateProveedorDTO.Telefono.Trim();

                if (updateProveedorDTO.Direccion != null)
                    proveedor.Direccion = updateProveedorDTO.Direccion.Trim();

                if (updateProveedorDTO.TipoServicio != null)
                    proveedor.TipoServicio = updateProveedorDTO.TipoServicio.Trim().ToLower();

                if (updateProveedorDTO.Contacto != null)
                    proveedor.Contacto = updateProveedorDTO.Contacto.Trim();

                if (updateProveedorDTO.Email != null)
                    proveedor.Email = updateProveedorDTO.Email.Trim();

                proveedor.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProveedorExists(id))
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

        // DELETE: api/Proveedores/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                {
                    return NotFound($"Proveedor con ID {id} no encontrado");
                }

                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                // Capturar errores de integridad referencial
                var innerException = dbEx.InnerException;
                while (innerException != null)
                {
                    if (innerException.Message.Contains("foreign key") ||
                        innerException.Message.Contains("REFERENCE") ||
                        innerException.Message.Contains("1451") || // MySQL error code for foreign key constraint
                        innerException.Message.Contains("cannot delete"))
                    {
                        return BadRequest("No se puede eliminar el proveedor porque está siendo utilizado en otras partes del sistema. Elimine primero los registros relacionados.");
                    }
                    innerException = innerException.InnerException;
                }
        
                return StatusCode(500, $"Error al eliminar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.Id == id);
        }

        private bool IsValidTipoServicio(string tipoServicio)
        {
            var tiposValidos = new[] 
            { 
                "insumos", "maquinaria", "transporte", "otros" 
            };
            return tiposValidos.Contains(tipoServicio.ToLower());
        }
    }
}