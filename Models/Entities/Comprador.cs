using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Comprador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Ruc { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        public string? Contacto { get; set; }

        public string? Email { get; set; }

        public string? TipoComprador { get; set; } // "mayorista", "minorista", "industria", "exportador"

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        public Comprador()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}