using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Cosecha
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CultivoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CantidadKilos { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoCosecha { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CultivoId")]
        public virtual Cultivo Cultivo { get; set; } = null!;

        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        public Cosecha()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}