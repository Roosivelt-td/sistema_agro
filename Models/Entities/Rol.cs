using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Descripcion { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}