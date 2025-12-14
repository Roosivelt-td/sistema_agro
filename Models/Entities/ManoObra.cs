using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class ManoObra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProcesoId { get; set; }

        [Required]
        public int NumeroPeones { get; set; }

        [Required]
        public int DiasTrabajo { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoPorDia { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoTotal { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        [ForeignKey("ProcesoId")]
        public virtual ProcesoAgricola ProcesoAgricola { get; set; } = null!;

        public ManoObra()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}