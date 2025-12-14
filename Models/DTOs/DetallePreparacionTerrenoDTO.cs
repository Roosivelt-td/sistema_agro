using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class DetallePreparacionTerrenoDTO
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public string TipoPreparacion { get; set; } = string.Empty;
        public decimal HorasMaquinaria { get; set; }
        public decimal Costo { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Información relacionada
        public string? ProcesoTipo { get; set; }
        public string? CultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
    }

    public class CreateDetallePreparacionTerrenoDTO
    {
        [Required]
        public int ProcesoId { get; set; }

        [Required]
        public string TipoPreparacion { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Las horas no pueden ser negativas")]
        public decimal HorasMaquinaria { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal Costo { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateDetallePreparacionTerrenoDTO
    {
        public string? TipoPreparacion { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Las horas no pueden ser negativas")]
        public decimal? HorasMaquinaria { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal? Costo { get; set; }
        
        public string? Observaciones { get; set; }
    }
}