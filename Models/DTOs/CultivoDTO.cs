using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class CultivoDTO
    {
        public int Id { get; set; }
        public int TerrenoId { get; set; }
        public int TipoCultivoId { get; set; }
        public DateTime FechaSiembra { get; set; }
        public DateTime FechaCosechaEstimada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Información relacionada
        public string? TerrenoNombre { get; set; }
        public string? TipoCultivoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
        public int? DiasRestantes { get; set; }
        public bool? EstaAtrasado { get; set; }
    }

    public class CreateCultivoDTO
    {
        [Required]
        public int TerrenoId { get; set; }

        [Required]
        public int TipoCultivoId { get; set; }

        [Required]
        public DateTime FechaSiembra { get; set; }

        [Required]
        public string Estado { get; set; } = "planificado";
    }

    public class UpdateCultivoDTO
    {
        public DateTime? FechaSiembra { get; set; }
        public DateTime? FechaCosechaEstimada { get; set; }
        public string? Estado { get; set; }
    }
}