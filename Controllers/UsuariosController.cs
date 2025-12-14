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
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        [Authorize(Roles = "admin")] // ← Solo admin puede ver todos los usuarios
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new UsuarioDTO
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Rol = u.Rol,
                        Nombre = u.Nombre,
                        Apellidos = u.Apellidos,
                        Telefono = u.Telefono,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            // Solo permitir ver el propio perfil o si es admin
            var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserId != id && currentUserRole != "admin")
                return Forbid();
            
            try
            {
                var usuario = await _context.Usuarios
                    .Where(u => u.Id == id)
                    .Select(u => new UsuarioDTO
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Rol = u.Rol,
                        Nombre = u.Nombre,
                        Apellidos = u.Apellidos,
                        Telefono = u.Telefono,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                return usuario;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Usuarios/profile
        [HttpGet("profile")]
        public async Task<ActionResult<UsuarioDTO>> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                var usuario = await _context.Usuarios
                    .Where(u => u.Id == userId)
                    .Select(u => new UsuarioDTO
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Rol = u.Rol,
                        Nombre = u.Nombre,
                        Apellidos = u.Apellidos,
                        Telefono = u.Telefono,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return NotFound("Usuario no encontrado");

                return usuario;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        
        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, UpdateUsuarioDTO updateUsuarioDTO)
        {
            try
            {
                // Solo permitir actualizar el propio perfil o si es admin
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != id && currentUserRole != "admin")
                    return Forbid();

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                // Validar rol si se está actualizando
                if (updateUsuarioDTO.Rol != null && !IsValidRol(updateUsuarioDTO.Rol))
                {
                    return BadRequest("Rol no válido. Los roles permitidos son: admin, agricultor, supervisor");
                }

                // Verificar si el email ya existe (si se está actualizando)
                if (updateUsuarioDTO.Email != null && updateUsuarioDTO.Email != usuario.Email)
                {
                    if (await _context.Usuarios.AnyAsync(u => u.Email == updateUsuarioDTO.Email && u.Id != id))
                    {
                        return BadRequest("El email ya está registrado por otro usuario");
                    }
                    usuario.Email = updateUsuarioDTO.Email.Trim();
                }

                // Actualizar solo los campos que se proporcionaron
                if (updateUsuarioDTO.Password != null)
                    usuario.PasswordHash = updateUsuarioDTO.Password;

                if (updateUsuarioDTO.Rol != null)
                    usuario.Rol = updateUsuarioDTO.Rol.Trim();

                if (updateUsuarioDTO.Nombre != null)
                    usuario.Nombre = updateUsuarioDTO.Nombre.Trim();

                if (updateUsuarioDTO.Apellidos != null)
                    usuario.Apellidos = updateUsuarioDTO.Apellidos.Trim();

                if (updateUsuarioDTO.Telefono != null)
                    usuario.Telefono = updateUsuarioDTO.Telefono.Trim();

                // Actualizar fecha de modificación
                usuario.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ← Solo admin puede eliminar usuarios
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                _context.Usuarios.Remove(usuario);
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

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        private bool IsValidRol(string rol)
        {
            return rol == "admin" || rol == "agricultor" || rol == "supervisor";
        }
    }
}