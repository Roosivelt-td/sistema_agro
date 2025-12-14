using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class InsumoUtilizado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProcesoId { get; set; }

        [Required]
        public int TipoInsumoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoFlete { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }
        
        // NUEVO: Relación opcional con proveedor
        public int? ProveedorId { get; set; }

        // Navigation properties
        [ForeignKey("ProcesoId")]
        public virtual ProcesoAgricola ProcesoAgricola { get; set; } = null!;

        [ForeignKey("TipoInsumoId")]
        public virtual TipoInsumo TipoInsumo { get; set; } = null!;

        // Navigation property
        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; } 
        public InsumoUtilizado()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // Propiedad calculada
        [NotMapped]
        public decimal CostoTotal => (Cantidad * CostoUnitario) + CostoFlete;
    }
}