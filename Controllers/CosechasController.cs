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
    public class CosechasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CosechasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cosechas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CosechaDTO>>> GetCosechas()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Cosecha> query = _context.Cosechas;

                // Si no es admin, solo ver sus cosechas
                if (currentUserRole != "admin")
                {
                    query = query.Where(c => c.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var cosechas = await query
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Include(c => c.Ventas)
                    .Select(c => new CosechaDTO
                    {
                        Id = c.Id,
                        CultivoId = c.CultivoId,
                        Fecha = c.Fecha,
                        CantidadKilos = c.CantidadKilos,
                        CostoCosecha = c.CostoCosecha,
                        Observaciones = c.Observaciones,
                        CreatedAt = c.CreatedAt,
                        CultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TipoCultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = c.Cultivo.Terreno.Nombre,
                        AgricultorNombre = c.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        KilosVendidos = c.Ventas.Sum(v => v.Cantidad),
                        TotalVendido = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg)
                    })
                    .ToListAsync();

                // Calcular kilos disponibles
                foreach (var cosecha in cosechas)
                {
                    cosecha.KilosDisponibles = cosecha.CantidadKilos - cosecha.KilosVendidos;
                }

                return Ok(cosechas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Cosechas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CosechaDTO>> GetCosecha(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cosecha = await _context.Cosechas
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Include(c => c.Ventas)
                    .Where(c => c.Id == id)
                    .Select(c => new CosechaDTO
                    {
                        Id = c.Id,
                        CultivoId = c.CultivoId,
                        Fecha = c.Fecha,
                        CantidadKilos = c.CantidadKilos,
                        CostoCosecha = c.CostoCosecha,
                        Observaciones = c.Observaciones,
                        CreatedAt = c.CreatedAt,
                        CultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TipoCultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = c.Cultivo.Terreno.Nombre,
                        AgricultorNombre = c.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        KilosVendidos = c.Ventas.Sum(v => v.Cantidad),
                        TotalVendido = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg)
                    })
                    .FirstOrDefaultAsync();

                if (cosecha == null)
                {
                    return NotFound($"Cosecha con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.Cultivos
                        .Where(c => c.Id == cosecha.CultivoId)
                        .Select(c => c.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                // Calcular kilos disponibles
                cosecha.KilosDisponibles = cosecha.CantidadKilos - cosecha.KilosVendidos;

                return cosecha;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Cosechas/cultivo/5
        [HttpGet("cultivo/{cultivoId}")]
        public async Task<ActionResult<IEnumerable<CosechaDTO>>> GetCosechasByCultivo(int cultivoId)
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

                var cosechas = await _context.Cosechas
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Include(c => c.Ventas)
                    .Where(c => c.CultivoId == cultivoId)
                    .Select(c => new CosechaDTO
                    {
                        Id = c.Id,
                        CultivoId = c.CultivoId,
                        Fecha = c.Fecha,
                        CantidadKilos = c.CantidadKilos,
                        CostoCosecha = c.CostoCosecha,
                        Observaciones = c.Observaciones,
                        CreatedAt = c.CreatedAt,
                        CultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TipoCultivoNombre = c.Cultivo.TipoCultivo.Nombre,
                        TerrenoNombre = c.Cultivo.Terreno.Nombre,
                        AgricultorNombre = c.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        KilosVendidos = c.Ventas.Sum(v => v.Cantidad),
                        TotalVendido = c.Ventas.Sum(v => v.Cantidad * v.PrecioKg)
                    })
                    .ToListAsync();

                // Calcular kilos disponibles
                foreach (var cosecha in cosechas)
                {
                    cosecha.KilosDisponibles = cosecha.CantidadKilos - cosecha.KilosVendidos;
                }

                return Ok(cosechas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Cosechas
        [HttpPost]
        public async Task<ActionResult<CosechaDTO>> PostCosecha(CreateCosechaDTO createCosechaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createCosechaDTO.Fecha == default)
                {
                    return BadRequest("Fecha es un campo requerido");
                }

                if (createCosechaDTO.CantidadKilos <= 0)
                {
                    return BadRequest("La cantidad de kilos debe ser mayor a 0");
                }

                // Verificar si el cultivo existe
                var cultivo = await _context.Cultivos
                    .Include(c => c.TipoCultivo)
                    .Include(c => c.Terreno)
                        .ThenInclude(t => t.Agricultor)
                            .ThenInclude(a => a.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == createCosechaDTO.CultivoId);
                
                if (cultivo == null)
                {
                    return BadRequest("El cultivo especificado no existe");
                }

                // Verificar permisos del cultivo
                if (currentUserRole != "admin" && cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validar que la fecha de cosecha no sea anterior a la fecha de siembra
                if (createCosechaDTO.Fecha < cultivo.FechaSiembra)
                {
                    return BadRequest("La fecha de cosecha no puede ser anterior a la fecha de siembra del cultivo");
                }

                var cosecha = new Cosecha
                {
                    CultivoId = createCosechaDTO.CultivoId,
                    Fecha = createCosechaDTO.Fecha.Date,
                    CantidadKilos = createCosechaDTO.CantidadKilos,
                    CostoCosecha = createCosechaDTO.CostoCosecha,
                    Observaciones = createCosechaDTO.Observaciones?.Trim()
                };

                _context.Cosechas.Add(cosecha);
                await _context.SaveChangesAsync();

                var cosechaDTO = new CosechaDTO
                {
                    Id = cosecha.Id,
                    CultivoId = cosecha.CultivoId,
                    Fecha = cosecha.Fecha,
                    CantidadKilos = cosecha.CantidadKilos,
                    CostoCosecha = cosecha.CostoCosecha,
                    Observaciones = cosecha.Observaciones,
                    CreatedAt = cosecha.CreatedAt,
                    CultivoNombre = cultivo.TipoCultivo.Nombre,
                    TipoCultivoNombre = cultivo.TipoCultivo.Nombre,
                    TerrenoNombre = cultivo.Terreno.Nombre,
                    AgricultorNombre = cultivo.Terreno.Agricultor.Usuario.Nombre,
                    KilosVendidos = 0,
                    TotalVendido = 0,
                    KilosDisponibles = cosecha.CantidadKilos
                };

                return CreatedAtAction(nameof(GetCosecha), new { id = cosecha.Id }, cosechaDTO);
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

        // PUT: api/Cosechas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCosecha(int id, UpdateCosechaDTO updateCosechaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cosecha = await _context.Cosechas
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                    .Include(c => c.Ventas)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (cosecha == null)
                {
                    return NotFound($"Cosecha con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && cosecha.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validar fecha si se está actualizando
                if (updateCosechaDTO.Fecha.HasValue)
                {
                    if (updateCosechaDTO.Fecha.Value < cosecha.Cultivo.FechaSiembra)
                    {
                        return BadRequest("La fecha de cosecha no puede ser anterior a la fecha de siembra del cultivo");
                    }
                    cosecha.Fecha = updateCosechaDTO.Fecha.Value.Date;
                }

                // Validar cantidad de kilos si se está actualizando
                if (updateCosechaDTO.CantidadKilos.HasValue)
                {
                    var kilosVendidos = cosecha.Ventas.Sum(v => v.Cantidad);
                    if (updateCosechaDTO.CantidadKilos.Value < kilosVendidos)
                    {
                        return BadRequest($"No se puede reducir la cantidad de kilos por debajo de los kilos ya vendidos ({kilosVendidos} kg)");
                    }
                    cosecha.CantidadKilos = updateCosechaDTO.CantidadKilos.Value;
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateCosechaDTO.CostoCosecha.HasValue)
                    cosecha.CostoCosecha = updateCosechaDTO.CostoCosecha.Value;

                if (updateCosechaDTO.Observaciones != null)
                    cosecha.Observaciones = updateCosechaDTO.Observaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CosechaExists(id))
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

        // DELETE: api/Cosechas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCosecha(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var cosecha = await _context.Cosechas
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                    .Include(c => c.Ventas)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (cosecha == null)
                {
                    return NotFound($"Cosecha con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && cosecha.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si hay ventas asociadas
                if (cosecha.Ventas.Any())
                {
                    return BadRequest("No se puede eliminar la cosecha porque tiene ventas asociadas. Elimine primero las ventas.");
                }

                _context.Cosechas.Remove(cosecha);
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

        private bool CosechaExists(int id)
        {
            return _context.Cosechas.Any(e => e.Id == id);
        }
    }
}