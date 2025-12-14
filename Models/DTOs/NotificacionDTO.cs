using System.ComponentModel.DataAnnotations;

namespace SistemaGestionAgricola.Models.DTOs
{
    public class NotificacionDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CultivoId { get; set; }
        
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        
        public string Mensaje { get; set; } = string.Empty;
        
        [DataType(DataType.Date)]
        public DateTime FechaProgramada { get; set; }
        
        [StringLength(20)]
        public string Estado { get; set; } = string.Empty;
        
        [DataType(DataType.DateTime)]
        public DateTime? FechaEnvio { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        
        // Información relacionada
        public string? UsuarioNombre { get; set; }
        public string? UsuarioEmail { get; set; }
        public string? CultivoNombre { get; set; }
        public string? TerrenoNombre { get; set; }
        public string? AgricultorNombre { get; set; }
    }

    public class CreateNotificacionDTO
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El ID de cultivo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de cultivo debe ser mayor a 0")]
        public int CultivoId { get; set; }

        [Required(ErrorMessage = "El tipo de notificación es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha programada es requerida")]
        [DataType(DataType.Date)]
        public DateTime FechaProgramada { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "pendiente";
    }

    public class UpdateNotificacionDTO
    {
        [StringLength(20)]
        public string? Estado { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? FechaEnvio { get; set; }
    }

    // Validador personalizado para fecha futura/hoy - CORREGIDO
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "La fecha no puede ser en el pasado");
                }
            }
            return ValidationResult.Success;
        }
    }
}