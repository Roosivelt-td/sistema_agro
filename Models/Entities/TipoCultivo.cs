using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class TipoCultivo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int TiempoSiembraCosecha { get; set; } // en días

        public string? InstruccionesRiegos { get; set; }

        public string? InstruccionesFumigaciones { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Cultivo> Cultivos { get; set; } = new List<Cultivo>();
        public virtual ICollection<NotificacionAutomatica> NotificacionesAutomaticas { get; set; } = new List<NotificacionAutomatica>();
        public TipoCultivo()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}