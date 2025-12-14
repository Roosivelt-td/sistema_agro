using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class TipoProceso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ProcesoAgricola> ProcesosAgricolas { get; set; } = new List<ProcesoAgricola>();

        public TipoProceso()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}