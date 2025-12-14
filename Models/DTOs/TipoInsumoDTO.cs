using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class TipoInsumoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTipoInsumoDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Categoria { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }

    public class UpdateTipoInsumoDTO
    {
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public string? Descripcion { get; set; }
    }
}