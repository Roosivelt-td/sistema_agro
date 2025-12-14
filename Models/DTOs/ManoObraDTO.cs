using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class ManoObraDTO
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public int NumeroPeones { get; set; }
        public int DiasTrabajo { get; set; }
        public decimal CostoPorDia { get; set; }
        public decimal CostoTotal { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Información relacionada
        public string? ProcesoTipo { get; set; }
        public string? CultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
    }

    public class CreateManoObraDTO
    {
        [Required]
        public int ProcesoId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "El número de peones debe estar entre 1 y 100")]
        public int NumeroPeones { get; set; }

        [Required]
        [Range(1, 365, ErrorMessage = "Los días de trabajo deben estar entre 1 y 365")]
        public int DiasTrabajo { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El costo por día no puede ser negativo")]
        public decimal CostoPorDia { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateManoObraDTO
    {
        [Range(1, 100, ErrorMessage = "El número de peones debe estar entre 1 y 100")]
        public int? NumeroPeones { get; set; }
        
        [Range(1, 365, ErrorMessage = "Los días de trabajo deben estar entre 1 y 365")]
        public int? DiasTrabajo { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo por día no puede ser negativo")]
        public decimal? CostoPorDia { get; set; }
        
        public string? Observaciones { get; set; }
    }
}