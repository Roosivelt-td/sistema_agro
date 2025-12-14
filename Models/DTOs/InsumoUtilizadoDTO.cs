using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class InsumoUtilizadoDTO
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public int TipoInsumoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoFlete { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Propiedades calculadas
        public decimal CostoTotal { get; set; }
        
        // Información relacionada
        public string? TipoInsumoNombre { get; set; }
        public string? TipoInsumoCategoria { get; set; }
        public string? ProcesoTipo { get; set; }
        public string? CultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
    }

    public class CreateInsumoUtilizadoDTO
    {
        [Required]
        public int ProcesoId { get; set; }

        [Required]
        public int TipoInsumoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El costo unitario no puede ser negativo")]
        public decimal CostoUnitario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo de flete no puede ser negativo")]
        public decimal CostoFlete { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateInsumoUtilizadoDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal? Cantidad { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo unitario no puede ser negativo")]
        public decimal? CostoUnitario { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo de flete no puede ser negativo")]
        public decimal? CostoFlete { get; set; }
        
        public string? Observaciones { get; set; }
    }
}