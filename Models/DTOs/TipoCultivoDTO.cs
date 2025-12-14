using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class TipoCultivoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TiempoSiembraCosecha { get; set; }
        public string? InstruccionesRiegos { get; set; }
        public string? InstruccionesFumigaciones { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTipoCultivoDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(1, 365, ErrorMessage = "El tiempo de siembra a cosecha debe estar entre 1 y 365 días")]
        public int TiempoSiembraCosecha { get; set; }

        public string? InstruccionesRiegos { get; set; }
        public string? InstruccionesFumigaciones { get; set; }
    }

    public class UpdateTipoCultivoDTO
    {
        public string? Nombre { get; set; }
        
        [Range(1, 365, ErrorMessage = "El tiempo de siembra a cosecha debe estar entre 1 y 365 días")]
        public int? TiempoSiembraCosecha { get; set; }
        
        public string? InstruccionesRiegos { get; set; }
        public string? InstruccionesFumigaciones { get; set; }
    }
}