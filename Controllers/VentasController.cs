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
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentas()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Venta> query = _context.Ventas;

                // Si no es admin, solo ver sus ventas
                if (currentUserRole != "admin")
                {
                    query = query.Where(v => v.Cosecha.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var ventas = await query
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.TipoCultivo)
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Include(v => v.Comprador)
                    .Select(v => new VentaDTO
                    {
                        Id = v.Id,
                        CosechaId = v.CosechaId,
                        CompradorId = v.CompradorId,
                        Fecha = v.Fecha,
                        Cantidad = v.Cantidad,
                        PrecioKg = v.PrecioKg,
                        CostoFlete = v.CostoFlete,
                        Observaciones = v.Observaciones,
                        CreatedAt = v.CreatedAt,
                        IngresoBruto = v.Cantidad * v.PrecioKg,
                        IngresoNeto = (v.Cantidad * v.PrecioKg) - v.CostoFlete,
                        CosechaCultivo = v.Cosecha.Cultivo.TipoCultivo.Nombre,
                        CompradorNombre = v.Comprador.Nombre,
                        CompradorTipo = v.Comprador.TipoComprador,
                        AgricultorNombre = v.Cosecha.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        TerrenoNombre = v.Cosecha.Cultivo.Terreno.Nombre
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDTO>> GetVenta(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var venta = await _context.Ventas
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.TipoCultivo)
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Include(v => v.Comprador)
                    .Where(v => v.Id == id)
                    .Select(v => new VentaDTO
                    {
                        Id = v.Id,
                        CosechaId = v.CosechaId,
                        CompradorId = v.CompradorId,
                        Fecha = v.Fecha,
                        Cantidad = v.Cantidad,
                        PrecioKg = v.PrecioKg,
                        CostoFlete = v.CostoFlete,
                        Observaciones = v.Observaciones,
                        CreatedAt = v.CreatedAt,
                        IngresoBruto = v.Cantidad * v.PrecioKg,
                        IngresoNeto = (v.Cantidad * v.PrecioKg) - v.CostoFlete,
                        CosechaCultivo = v.Cosecha.Cultivo.TipoCultivo.Nombre,
                        CompradorNombre = v.Comprador.Nombre,
                        CompradorTipo = v.Comprador.TipoComprador,
                        AgricultorNombre = v.Cosecha.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        TerrenoNombre = v.Cosecha.Cultivo.Terreno.Nombre
                    })
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    return NotFound($"Venta con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin")
                {
                    var agricultorUsuarioId = await _context.Cosechas
                        .Where(c => c.Id == venta.CosechaId)
                        .Select(c => c.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (agricultorUsuarioId != currentUserId)
                        return Forbid();
                }

                return venta;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Ventas/cosecha/5
        [HttpGet("cosecha/{cosechaId}")]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByCosecha(int cosechaId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Verificar permisos de la cosecha
                if (currentUserRole != "admin")
                {
                    var cosechaUsuarioId = await _context.Cosechas
                        .Where(c => c.Id == cosechaId)
                        .Select(c => c.Cultivo.Terreno.Agricultor.UsuarioId)
                        .FirstOrDefaultAsync();

                    if (cosechaUsuarioId != currentUserId)
                        return Forbid();
                }

                var ventas = await _context.Ventas
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.TipoCultivo)
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Include(v => v.Comprador)
                    .Where(v => v.CosechaId == cosechaId)
                    .Select(v => new VentaDTO
                    {
                        Id = v.Id,
                        CosechaId = v.CosechaId,
                        CompradorId = v.CompradorId,
                        Fecha = v.Fecha,
                        Cantidad = v.Cantidad,
                        PrecioKg = v.PrecioKg,
                        CostoFlete = v.CostoFlete,
                        Observaciones = v.Observaciones,
                        CreatedAt = v.CreatedAt,
                        IngresoBruto = v.Cantidad * v.PrecioKg,
                        IngresoNeto = (v.Cantidad * v.PrecioKg) - v.CostoFlete,
                        CosechaCultivo = v.Cosecha.Cultivo.TipoCultivo.Nombre,
                        CompradorNombre = v.Comprador.Nombre,
                        CompradorTipo = v.Comprador.TipoComprador,
                        AgricultorNombre = v.Cosecha.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        TerrenoNombre = v.Cosecha.Cultivo.Terreno.Nombre
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Ventas/comprador/5
        [HttpGet("comprador/{compradorId}")]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByComprador(int compradorId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Venta> query = _context.Ventas.Where(v => v.CompradorId == compradorId);

                // Si no es admin, solo ver sus ventas
                if (currentUserRole != "admin")
                {
                    query = query.Where(v => v.Cosecha.Cultivo.Terreno.Agricultor.UsuarioId == currentUserId);
                }

                var ventas = await query
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.TipoCultivo)
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                                .ThenInclude(t => t.Agricultor)
                                    .ThenInclude(a => a.Usuario)
                    .Include(v => v.Comprador)
                    .Select(v => new VentaDTO
                    {
                        Id = v.Id,
                        CosechaId = v.CosechaId,
                        CompradorId = v.CompradorId,
                        Fecha = v.Fecha,
                        Cantidad = v.Cantidad,
                        PrecioKg = v.PrecioKg,
                        CostoFlete = v.CostoFlete,
                        Observaciones = v.Observaciones,
                        CreatedAt = v.CreatedAt,
                        IngresoBruto = v.Cantidad * v.PrecioKg,
                        IngresoNeto = (v.Cantidad * v.PrecioKg) - v.CostoFlete,
                        CosechaCultivo = v.Cosecha.Cultivo.TipoCultivo.Nombre,
                        CompradorNombre = v.Comprador.Nombre,
                        CompradorTipo = v.Comprador.TipoComprador,
                        AgricultorNombre = v.Cosecha.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                        TerrenoNombre = v.Cosecha.Cultivo.Terreno.Nombre
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Ventas
        [HttpPost]
        public async Task<ActionResult<VentaDTO>> PostVenta(CreateVentaDTO createVentaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar campos requeridos
                if (createVentaDTO.Fecha == default)
                {
                    return BadRequest("Fecha es un campo requerido");
                }

                if (createVentaDTO.Cantidad <= 0)
                {
                    return BadRequest("La cantidad debe ser mayor a 0");
                }

                if (createVentaDTO.PrecioKg <= 0)
                {
                    return BadRequest("El precio por kg debe ser mayor a 0");
                }

                // Verificar si la cosecha existe
                var cosecha = await _context.Cosechas
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.TipoCultivo)
                    .Include(c => c.Cultivo)
                        .ThenInclude(c => c.Terreno)
                            .ThenInclude(t => t.Agricultor)
                                .ThenInclude(a => a.Usuario)
                    .Include(c => c.Ventas)
                    .FirstOrDefaultAsync(c => c.Id == createVentaDTO.CosechaId);
                
                if (cosecha == null)
                {
                    return BadRequest("La cosecha especificada no existe");
                }

                // Verificar permisos de la cosecha
                if (currentUserRole != "admin" && cosecha.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Verificar si el comprador existe
                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.Id == createVentaDTO.CompradorId);
                
                if (comprador == null)
                {
                    return BadRequest("El comprador especificado no existe");
                }

                // Calcular kilos disponibles en la cosecha
                var kilosVendidos = cosecha.Ventas.Sum(v => v.Cantidad);
                var kilosDisponibles = cosecha.CantidadKilos - kilosVendidos;

                // Validar que la cantidad a vender no exceda los kilos disponibles
                if (createVentaDTO.Cantidad > kilosDisponibles)
                {
                    return BadRequest($"No hay suficientes kilos disponibles. Disponibles: {kilosDisponibles} kg, Solicitados: {createVentaDTO.Cantidad} kg");
                }

                // Validar que la fecha de venta no sea anterior a la fecha de cosecha
                if (createVentaDTO.Fecha < cosecha.Fecha)
                {
                    return BadRequest("La fecha de venta no puede ser anterior a la fecha de cosecha");
                }

                var venta = new Venta
                {
                    CosechaId = createVentaDTO.CosechaId,
                    CompradorId = createVentaDTO.CompradorId,
                    Fecha = createVentaDTO.Fecha.Date,
                    Cantidad = createVentaDTO.Cantidad,
                    PrecioKg = createVentaDTO.PrecioKg,
                    CostoFlete = createVentaDTO.CostoFlete,
                    Observaciones = createVentaDTO.Observaciones?.Trim()
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                var ventaDTO = new VentaDTO
                {
                    Id = venta.Id,
                    CosechaId = venta.CosechaId,
                    CompradorId = venta.CompradorId,
                    Fecha = venta.Fecha,
                    Cantidad = venta.Cantidad,
                    PrecioKg = venta.PrecioKg,
                    CostoFlete = venta.CostoFlete,
                    Observaciones = venta.Observaciones,
                    CreatedAt = venta.CreatedAt,
                    IngresoBruto = venta.Cantidad * venta.PrecioKg,
                    IngresoNeto = (venta.Cantidad * venta.PrecioKg) - venta.CostoFlete,
                    CosechaCultivo = cosecha.Cultivo.TipoCultivo.Nombre,
                    CompradorNombre = comprador.Nombre,
                    CompradorTipo = comprador.TipoComprador,
                    AgricultorNombre = cosecha.Cultivo.Terreno.Agricultor.Usuario.Nombre,
                    TerrenoNombre = cosecha.Cultivo.Terreno.Nombre
                };

                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, ventaDTO);
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

        // PUT: api/Ventas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, UpdateVentaDTO updateVentaDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var venta = await _context.Ventas
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Ventas)
                    .FirstOrDefaultAsync(v => v.Id == id);
                
                if (venta == null)
                {
                    return NotFound($"Venta con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && venta.Cosecha.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                // Validar cantidad si se está actualizando
                if (updateVentaDTO.Cantidad.HasValue)
                {
                    // Calcular kilos disponibles excluyendo la venta actual
                    var kilosVendidosExcluyendoActual = venta.Cosecha.Ventas
                        .Where(v => v.Id != id)
                        .Sum(v => v.Cantidad);
                    
                    var kilosDisponibles = venta.Cosecha.CantidadKilos - kilosVendidosExcluyendoActual;

                    if (updateVentaDTO.Cantidad.Value > kilosDisponibles)
                    {
                        return BadRequest($"No hay suficientes kilos disponibles. Disponibles: {kilosDisponibles} kg, Solicitados: {updateVentaDTO.Cantidad.Value} kg");
                    }
                    venta.Cantidad = updateVentaDTO.Cantidad.Value;
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateVentaDTO.Fecha.HasValue)
                    venta.Fecha = updateVentaDTO.Fecha.Value.Date;

                if (updateVentaDTO.PrecioKg.HasValue)
                    venta.PrecioKg = updateVentaDTO.PrecioKg.Value;

                if (updateVentaDTO.CostoFlete.HasValue)
                    venta.CostoFlete = updateVentaDTO.CostoFlete.Value;

                if (updateVentaDTO.Observaciones != null)
                    venta.Observaciones = updateVentaDTO.Observaciones.Trim();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(id))
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

        // DELETE: api/Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var venta = await _context.Ventas
                    .Include(v => v.Cosecha)
                        .ThenInclude(c => c.Cultivo)
                            .ThenInclude(c => c.Terreno)
                    .FirstOrDefaultAsync(v => v.Id == id);
                
                if (venta == null)
                {
                    return NotFound($"Venta con ID {id} no encontrada");
                }

                // Verificar permisos
                if (currentUserRole != "admin" && venta.Cosecha.Cultivo.Terreno.Agricultor.UsuarioId != currentUserId)
                    return Forbid();

                _context.Ventas.Remove(venta);
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

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}