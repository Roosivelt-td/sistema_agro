using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Agricultor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Dni { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public string? Experiencia { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        // Collection navigation properties
        public virtual ICollection<Terreno> Terrenos { get; set; } = new List<Terreno>();

        public Agricultor()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}