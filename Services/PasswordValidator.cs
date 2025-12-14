using System.Text.RegularExpressions;

namespace SistemaGestionAgricola.Services
{
    public class PasswordValidator : IPasswordValidator
    {
        public (bool IsValid, string Message) ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
                errors.Add("La contraseña no puede estar vacía");

            if (password != null)
            {
                // Mínimo 8 caracteres
                if (password.Length < 8)
                    errors.Add("Debe tener al menos 8 caracteres");
                
                // Máximo 100 caracteres
                if (password.Length > 100)
                    errors.Add("No puede exceder 100 caracteres");
                
                // Letra mayúscula
                if (!Regex.IsMatch(password, @"[A-Z]"))
                    errors.Add("Debe contener al menos una letra mayúscula (A-Z)");
                
                // Letra minúscula
                if (!Regex.IsMatch(password, @"[a-z]"))
                    errors.Add("Debe contener al menos una letra minúscula (a-z)");
                
                // Dígito
                if (!Regex.IsMatch(password, @"\d"))
                    errors.Add("Debe contener al menos un número (0-9)");
                
                // Carácter especial
                if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                    errors.Add("Debe contener al menos un carácter especial (!@#$%^&* etc.)");
                
                // Sin espacios
                if (password.Contains(" "))
                    errors.Add("No debe contener espacios");
                
                // Contraseñas débiles comunes
                var weakPasswords = new[] { "password", "12345678", "qwerty", "admin", "welcome", "password123", "123456789" };
                if (weakPasswords.Contains(password.ToLower()))
                    errors.Add("Es demasiado común, elige una más segura");
            }

            if (errors.Count > 0)
            {
                var errorMessage = "La contraseña no cumple con los siguientes requisitos:\n" +
                                 string.Join("\n• ", errors);
                return (false, errorMessage);
            }

            return (true, "Contraseña válida");
        }
        
        // Método para obtener solo los requisitos (para mostrar en frontend)
        public string GetPasswordRequirements()
        {
            return @"🔒 La contraseña DEBE contener:
• Mínimo 8 caracteres
• Al menos una LETRA MAYÚSCULA (A-Z)
• Al menos una letra minúscula (a-z)
• Al menos un NÚMERO (0-9)
• Al menos un CARÁCTER ESPECIAL (!@#$%^&* etc.)
• SIN espacios
• No usar contraseñas comunes como 'password', '12345678', etc.";
        }
    }
}