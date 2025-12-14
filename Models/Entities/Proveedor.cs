using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.Entities
{
    public class Proveedor
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

        [Required]
        [MaxLength(50)]
        public string TipoServicio { get; set; } = string.Empty; // "insumos", "maquinaria", "transporte", "otros"

        public string? Contacto { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<InsumoUtilizado> InsumosUtilizados { get; set; } = new List<InsumoUtilizado>();

        public Proveedor()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}