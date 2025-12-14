using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class NotificacionAutomatica
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TipoCultivoId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoEvento { get; set; } = string.Empty; // "riego", "fumigacion", "cosecha", "mantenimiento"

        [Required]
        public int DiasDespuesSiembra { get; set; }

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("TipoCultivoId")]
        public virtual TipoCultivo TipoCultivo { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        public NotificacionAutomatica()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}