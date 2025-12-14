using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class TipoInsumo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty; // "semilla", "fertilizante", "herbicida", "insecticida", "comida_peones", "empaque", "otros"

        public string? Descripcion { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<InsumoUtilizado> InsumosUtilizados { get; set; } = new List<InsumoUtilizado>();

        public TipoInsumo()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}