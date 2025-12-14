using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class ProcesoAgricolaDTO
    {
        public int Id { get; set; }
        public int CultivoId { get; set; }
        public int TipoProcesoId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal CostoManoObra { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Información relacionada
        public string? TipoProcesoNombre { get; set; }
        public string? CultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
        
        // Totales calculados
        public decimal TotalInsumos { get; set; }
        public decimal TotalManoObra { get; set; }
        public decimal TotalProceso { get; set; }
    }

    public class CreateProcesoAgricolaDTO
    {
        [Required]
        public int CultivoId { get; set; }

        [Required]
        public int TipoProcesoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal CostoManoObra { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateProcesoAgricolaDTO
    {
        public DateTime? Fecha { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal? CostoManoObra { get; set; }
        
        public string? Observaciones { get; set; }
    }
}