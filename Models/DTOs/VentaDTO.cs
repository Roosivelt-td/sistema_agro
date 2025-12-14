using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public int CosechaId { get; set; }
        public int CompradorId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioKg { get; set; }
        public decimal CostoFlete { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Propiedades calculadas
        public decimal IngresoBruto { get; set; }
        public decimal IngresoNeto { get; set; }
        
        // Información relacionada
        public string? CosechaCultivo { get; set; }
        public string? CompradorNombre { get; set; }
        public string? CompradorTipo { get; set; }
        public string? AgricultorNombre { get; set; }
        public string? TerrenoNombre { get; set; }
    }

    public class CreateVentaDTO
    {
        [Required]
        public int CosechaId { get; set; }

        [Required]
        public int CompradorId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio por kg no puede ser negativo")]
        public decimal PrecioKg { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo de flete no puede ser negativo")]
        public decimal CostoFlete { get; set; }

        public string? Observaciones { get; set; }
    }

    public class UpdateVentaDTO
    {
        public DateTime? Fecha { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal? Cantidad { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El precio por kg no puede ser negativo")]
        public decimal? PrecioKg { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El costo de flete no puede ser negativo")]
        public decimal? CostoFlete { get; set; }
        
        public string? Observaciones { get; set; }
    }
}