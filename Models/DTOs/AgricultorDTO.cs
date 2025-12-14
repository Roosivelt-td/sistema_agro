using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class AgricultorDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Experiencia { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Información del usuario relacionado
        public string? UsuarioNombre { get; set; }
        public string? UsuarioEmail { get; set; }
        public string? UsuarioTelefono { get; set; }
    }

    public class CreateAgricultorDTO
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string Dni { get; set; } = string.Empty;

        public string? Direccion { get; set; }
        public string? Experiencia { get; set; }
    }

    public class UpdateAgricultorDTO
    {
        public string? Dni { get; set; }
        public string? Direccion { get; set; }
        public string? Experiencia { get; set; }
    }
}