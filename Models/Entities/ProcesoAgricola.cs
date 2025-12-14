using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class ProcesoAgricola
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CultivoId { get; set; }

        [Required]
        public int TipoProcesoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoManoObra { get; set; }

        public string? Observaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CultivoId")]
        public virtual Cultivo Cultivo { get; set; } = null!;

        [ForeignKey("TipoProcesoId")]
        public virtual TipoProceso TipoProceso { get; set; } = null!;

        
        public virtual ICollection<DetallePreparacionTerreno> DetallesPreparacionTerreno { get; set; } = new List<DetallePreparacionTerreno>();
        public virtual ICollection<InsumoUtilizado> InsumosUtilizados { get; set; } = new List<InsumoUtilizado>();
        public virtual ICollection<ManoObra> ManosObra { get; set; } = new List<ManoObra>();

        public ProcesoAgricola()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}