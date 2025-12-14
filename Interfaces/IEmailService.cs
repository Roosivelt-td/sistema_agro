namespace SistemaGestionAgricola.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendVerificationCodeAsync(string toEmail, string code);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task<bool> SendSystemNotificationAsync(string toEmail, string title, string message, string notificationType = "info");
    }
}