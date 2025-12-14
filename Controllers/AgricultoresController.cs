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
    [Authorize] // ← Proteger todo el controller
    public class AgricultoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgricultoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Agricultores
        [HttpGet]
        [Authorize(Roles = "admin")] // ← Solo admin puede ver todos los agricultores
        public async Task<ActionResult<IEnumerable<AgricultorDTO>>> GetAgricultores()
        {
            try
            {
                var agricultores = await _context.Agricultores
                    .Include(a => a.Usuario)
                    .Select(a => new AgricultorDTO
                    {
                        Id = a.Id,
                        UsuarioId = a.UsuarioId,
                        Dni = a.Dni,
                        Direccion = a.Direccion,
                        Experiencia = a.Experiencia,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        UsuarioNombre = a.Usuario.Nombre,
                        UsuarioEmail = a.Usuario.Email,
                        UsuarioTelefono = a.Usuario.Telefono
                    })
                    .ToListAsync();

                return Ok(agricultores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Agricultores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AgricultorDTO>> GetAgricultor(int id)
        {
            try
            {
                // Solo permitir ver el propio perfil de agricultor o si es admin
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Buscar el agricultor
                var agricultor = await _context.Agricultores
                    .Include(a => a.Usuario)
                    .Where(a => a.Id == id)
                    .Select(a => new AgricultorDTO
                    {
                        Id = a.Id,
                        UsuarioId = a.UsuarioId,
                        Dni = a.Dni,
                        Direccion = a.Direccion,
                        Experiencia = a.Experiencia,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        UsuarioNombre = a.Usuario.Nombre,
                        UsuarioEmail = a.Usuario.Email,
                        UsuarioTelefono = a.Usuario.Telefono
                    })
                    .FirstOrDefaultAsync();

                if (agricultor == null)
                {
                    return NotFound($"Agricultor con ID {id} no encontrado");
                }

                // Verificar permisos: solo el propio usuario o admin
                if (agricultor.UsuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                return agricultor;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Agricultores/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<AgricultorDTO>> GetAgricultorByUsuarioId(int usuarioId)
        {
            try
            {
                // Solo permitir ver el propio perfil o si es admin
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (usuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                var agricultor = await _context.Agricultores
                    .Include(a => a.Usuario)
                    .Where(a => a.UsuarioId == usuarioId)
                    .Select(a => new AgricultorDTO
                    {
                        Id = a.Id,
                        UsuarioId = a.UsuarioId,
                        Dni = a.Dni,
                        Direccion = a.Direccion,
                        Experiencia = a.Experiencia,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        UsuarioNombre = a.Usuario.Nombre,
                        UsuarioEmail = a.Usuario.Email,
                        UsuarioTelefono = a.Usuario.Telefono
                    })
                    .FirstOrDefaultAsync();

                if (agricultor == null)
                {
                    return NotFound($"No se encontró agricultor para el usuario con ID {usuarioId}");
                }

                return agricultor;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Agricultores/profile
        [HttpGet("profile")]
        public async Task<ActionResult<AgricultorDTO>> GetProfile()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                var agricultor = await _context.Agricultores
                    .Include(a => a.Usuario)
                    .Where(a => a.UsuarioId == currentUserId)
                    .Select(a => new AgricultorDTO
                    {
                        Id = a.Id,
                        UsuarioId = a.UsuarioId,
                        Dni = a.Dni,
                        Direccion = a.Direccion,
                        Experiencia = a.Experiencia,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        UsuarioNombre = a.Usuario.Nombre,
                        UsuarioEmail = a.Usuario.Email,
                        UsuarioTelefono = a.Usuario.Telefono
                    })
                    .FirstOrDefaultAsync();

                if (agricultor == null)
                    return NotFound("No se encontró perfil de agricultor para el usuario actual");

                return agricultor;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Agricultores
        [HttpPost]
        public async Task<ActionResult<AgricultorDTO>> PostAgricultor(CreateAgricultorDTO createAgricultorDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Solo permitir crear perfil para uno mismo, a menos que sea admin
                if (createAgricultorDTO.UsuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(createAgricultorDTO.Dni))
                {
                    return BadRequest("DNI es un campo requerido");
                }

                // Verificar si el usuario existe
                var usuario = await _context.Usuarios.FindAsync(createAgricultorDTO.UsuarioId);
                if (usuario == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                // Verificar si el usuario ya tiene un agricultor asociado
                if (await _context.Agricultores.AnyAsync(a => a.UsuarioId == createAgricultorDTO.UsuarioId))
                {
                    return BadRequest("Este usuario ya tiene un perfil de agricultor asociado");
                }

                // Verificar si el DNI ya existe
                if (await _context.Agricultores.AnyAsync(a => a.Dni == createAgricultorDTO.Dni))
                {
                    return BadRequest("El DNI ya está registrado");
                }

                var agricultor = new Agricultor
                {
                    UsuarioId = createAgricultorDTO.UsuarioId,
                    Dni = createAgricultorDTO.Dni.Trim(),
                    Direccion = createAgricultorDTO.Direccion?.Trim(),
                    Experiencia = createAgricultorDTO.Experiencia?.Trim()
                };

                _context.Agricultores.Add(agricultor);
                await _context.SaveChangesAsync();

                // Cargar datos del usuario para la respuesta
                await _context.Entry(agricultor)
                    .Reference(a => a.Usuario)
                    .LoadAsync();

                var agricultorDTO = new AgricultorDTO
                {
                    Id = agricultor.Id,
                    UsuarioId = agricultor.UsuarioId,
                    Dni = agricultor.Dni,
                    Direccion = agricultor.Direccion,
                    Experiencia = agricultor.Experiencia,
                    CreatedAt = agricultor.CreatedAt,
                    UpdatedAt = agricultor.UpdatedAt,
                    UsuarioNombre = agricultor.Usuario.Nombre,
                    UsuarioEmail = agricultor.Usuario.Email,
                    UsuarioTelefono = agricultor.Usuario.Telefono
                };

                return CreatedAtAction(nameof(GetAgricultor), new { id = agricultor.Id }, agricultorDTO);
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

        // PUT: api/Agricultores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAgricultor(int id, UpdateAgricultorDTO updateAgricultorDTO)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var agricultor = await _context.Agricultores.FindAsync(id);
                if (agricultor == null)
                {
                    return NotFound($"Agricultor con ID {id} no encontrado");
                }

                // Solo permitir actualizar el propio perfil o si es admin
                if (agricultor.UsuarioId != currentUserId && currentUserRole != "admin")
                    return Forbid();

                // Verificar si el DNI ya existe (si se está actualizando)
                if (updateAgricultorDTO.Dni != null && updateAgricultorDTO.Dni != agricultor.Dni)
                {
                    if (await _context.Agricultores.AnyAsync(a => a.Dni == updateAgricultorDTO.Dni && a.Id != id))
                    {
                        return BadRequest("El DNI ya está registrado por otro agricultor");
                    }
                    agricultor.Dni = updateAgricultorDTO.Dni.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateAgricultorDTO.Direccion != null)
                    agricultor.Direccion = updateAgricultorDTO.Direccion.Trim();

                if (updateAgricultorDTO.Experiencia != null)
                    agricultor.Experiencia = updateAgricultorDTO.Experiencia.Trim();

                agricultor.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AgricultorExists(id))
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

        // DELETE: api/Agricultores/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar agricultores
        public async Task<IActionResult> DeleteAgricultor(int id)
        {
            try
            {
                var agricultor = await _context.Agricultores.FindAsync(id);
                if (agricultor == null)
                {
                    return NotFound($"Agricultor con ID {id} no encontrado");
                }

                _context.Agricultores.Remove(agricultor);
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

        private bool AgricultorExists(int id)
        {
            return _context.Agricultores.Any(e => e.Id == id);
        }
    }
}