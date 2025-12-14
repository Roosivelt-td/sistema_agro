using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Cultivo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TerrenoId { get; set; }

        [Required]
        public int TipoCultivoId { get; set; }

        [Required]
        public DateTime FechaSiembra { get; set; }

        [Required]
        public DateTime FechaCosechaEstimada { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "planificado"; // "planificado", "activo", "completado", "cancelado"

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("TerrenoId")]
        public virtual Terreno Terreno { get; set; } = null!;

        [ForeignKey("TipoCultivoId")]
        public virtual TipoCultivo TipoCultivo { get; set; } = null!;

        public virtual ICollection<ProcesoAgricola> ProcesosAgricolas { get; set; } = new List<ProcesoAgricola>();
        public virtual ICollection<Cosecha> Cosechas { get; set; } = new List<Cosecha>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        

        public Cultivo()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}