using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class TerrenoDTO
    {
        public int Id { get; set; }
        public int AgricultorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public decimal AreaHectareas { get; set; }
        public string TipoTenencia { get; set; } = string.Empty;
        public decimal CostoAlquiler { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Información del agricultor relacionado
        public string? AgricultorNombre { get; set; }
        public string? AgricultorDni { get; set; }
        public string? UsuarioNombre { get; set; }
    }

    public class CreateTerrenoDTO
    {
        [Required]
        public int AgricultorId { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Ubicacion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El área debe ser mayor a 0")]
        public decimal AreaHectareas { get; set; }

        [Required]
        public string TipoTenencia { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal CostoAlquiler { get; set; }
    }

    public class UpdateTerrenoDTO
    {
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "El área debe ser mayor a 0")]
        public decimal? AreaHectareas { get; set; }
        
        public string? TipoTenencia { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal? CostoAlquiler { get; set; }
    }
}