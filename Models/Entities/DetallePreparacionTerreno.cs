using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class DetallePreparacionTerreno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProcesoId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TipoPreparacion { get; set; } = string.Empty; // "arado", "surcado", "nivelado", etc.

        [Column(TypeName = "decimal(5,2)")]
        public decimal HorasMaquinaria { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Costo { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        [ForeignKey("ProcesoId")]
        public virtual ProcesoAgricola ProcesoAgricola { get; set; } = null!;

        public DetallePreparacionTerreno()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}