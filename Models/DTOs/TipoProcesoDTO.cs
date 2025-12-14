using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class TipoProcesoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTipoProcesoDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }

    public class UpdateTipoProcesoDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}