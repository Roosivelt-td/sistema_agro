using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class NotificacionAutomaticaDTO
    {
        public int Id { get; set; }
        public int TipoCultivoId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public int DiasDespuesSiembra { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Información relacionada
        public string? TipoCultivoNombre { get; set; }
        public string? UsuarioNombre { get; set; }
        public string? UsuarioEmail { get; set; }
    }

    public class CreateNotificacionAutomaticaDTO
    {
        [Required]
        public int TipoCultivoId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string TipoEvento { get; set; } = string.Empty;

        [Required]
        [Range(1, 365, ErrorMessage = "Los días después de siembra deben estar entre 1 y 365")]
        public int DiasDespuesSiembra { get; set; }

        [Required]
        public string Mensaje { get; set; } = string.Empty;
    }

    public class UpdateNotificacionAutomaticaDTO
    {
        public string? TipoEvento { get; set; }

        [Range(1, 365, ErrorMessage = "Los días después de siembra deben estar entre 1 y 365")]
        public int? DiasDespuesSiembra { get; set; }

        public string? Mensaje { get; set; }
    }
}