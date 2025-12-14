using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CultivoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        [Required]
        public DateTime FechaProgramada { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "pendiente";

        public DateTime? FechaEnvio { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("CultivoId")]
        public virtual Cultivo Cultivo { get; set; } = null!;

        public Notificacion()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    // Enums para tipos y estados (NUEVO - Mejora la consistencia)
    public enum TipoNotificacion
    {
        Riego,
        Fumigacion,
        Cosecha,
        Alerta,
        Recordatorio
    }

    public enum EstadoNotificacion
    {
        Pendiente,
        Enviada,
        Leida,
        Cancelada
    }
}