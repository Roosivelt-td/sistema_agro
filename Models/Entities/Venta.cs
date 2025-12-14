using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CosechaId { get; set; }

        [Required]
        public int CompradorId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioKg { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoFlete { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CosechaId")]
        public virtual Cosecha Cosecha { get; set; } = null!;

        [ForeignKey("CompradorId")]
        public virtual Comprador Comprador { get; set; } = null!;

        public Venta()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // Propiedades calculadas
        [NotMapped]
        public decimal IngresoBruto => Cantidad * PrecioKg;

        [NotMapped]
        public decimal IngresoNeto => IngresoBruto - CostoFlete;
    }
}