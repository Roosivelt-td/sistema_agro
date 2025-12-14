using System.ComponentModel.DataAnnotations; 

namespace SistemaGestionAgricola.Models.Entities
{
    public class EmailVerification
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
        
        public int Attempts { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        [StringLength(50)]
        public string? VerificationType { get; set; } // "register", "login", "reset_password"
    }
}