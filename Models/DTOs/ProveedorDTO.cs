using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class ProveedorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string TipoServicio { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProveedorDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        [Required]
        public string TipoServicio { get; set; } = string.Empty;

        public string? Contacto { get; set; }
        public string? Email { get; set; }
    }

    public class UpdateProveedorDTO
    {
        public string? Nombre { get; set; }
        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? TipoServicio { get; set; }
        public string? Contacto { get; set; }
        public string? Email { get; set; }
    }
}