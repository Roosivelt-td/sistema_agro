using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class CosechaDTO
    {
        public int Id { get; set; }
        public int CultivoId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal CantidadKilos { get; set; }
        public decimal CostoCosecha { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Información relacionada
        public string? CultivoNombre { get; set; }
        public string? TipoCultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
        
        // Información de ventas
        public decimal TotalVendido { get; set; }
        public decimal KilosVendidos { get; set; }
        public decimal KilosDisponibles { get; set; }
    }

    public class CreateCosechaDTO
    {
        [Required]
        public int CultivoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal CantidadKilos { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal CostoCosecha { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateCosechaDTO
    {
        public DateTime? Fecha { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal? CantidadKilos { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo")]
        public decimal? CostoCosecha { get; set; }
        
        public string? Observaciones { get; set; }
    }
}