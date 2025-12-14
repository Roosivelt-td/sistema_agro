using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // CAMBIADO de Password a PasswordHash

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "agricultor"; // Valor por defecto

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)] public string Apellidos { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property para Agricultor 
        public virtual Agricultor? Agricultor { get; set; }
        public virtual ICollection<NotificacionAutomatica> NotificacionesAutomaticas { get; set; } = new List<NotificacionAutomatica>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public bool IsEmailVerified { get; set; } = false;
        
        // Password actualizacion
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        // final de password 
        public Usuario()
        {
            // Establecer valores por defecto en el constructor
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsEmailVerified = false; 
        }
    }
}