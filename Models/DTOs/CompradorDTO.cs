using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class CompradorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Contacto { get; set; }
        public string? Email { get; set; }
        public string? TipoComprador { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Estadísticas
        public int TotalCompras { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? UltimaCompra { get; set; }
    }

    public class CreateCompradorDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Contacto { get; set; }
        public string? Email { get; set; }
        public string? TipoComprador { get; set; }
    }

    public class UpdateCompradorDTO
    {
        public string? Nombre { get; set; }
        public string? Ruc { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Contacto { get; set; }
        public string? Email { get; set; }
        public string? TipoComprador { get; set; }
    }
}