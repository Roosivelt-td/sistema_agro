using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Terreno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AgricultorId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        public string? Ubicacion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AreaHectareas { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoTenencia { get; set; } = string.Empty; // "propio", "alquilado"

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoAlquiler { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AgricultorId")]
        public virtual Agricultor Agricultor { get; set; } = null!;

        public virtual ICollection<Cultivo> Cultivos { get; set; } = new List<Cultivo>();

        public Terreno()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}