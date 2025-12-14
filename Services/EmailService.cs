using System.Net;
using System.Net.Mail;
using System.Security.Authentication;
using Microsoft.Extensions.Options;

namespace SistemaGestionAgricola.Services
{
    // 1. Configuración de Email
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Sistema de Gestión Agrícola";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public int Timeout { get; set; } = 30000;
    }

    // 2. Implementación del servicio de Email
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailConfiguration> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            
            // Log de configuración
            _logger.LogInformation("=== 🛠️ CONFIGURACIÓN EmailService ===");
            _logger.LogInformation($"SmtpServer: '{_settings.SmtpServer ?? "NULL"}'");
            _logger.LogInformation($"SmtpPort: {_settings.SmtpPort}");
            _logger.LogInformation($"SenderEmail: '{_settings.SenderEmail ?? "NULL"}'");
            _logger.LogInformation($"Username: '{_settings.Username ?? "NULL"}'");
            _logger.LogInformation($"Password configurada: {!string.IsNullOrEmpty(_settings.Password)}");
            _logger.LogInformation($"EnableSsl: {_settings.EnableSsl}");
            _logger.LogInformation("===================================");
        }

        // Método base para enviar cualquier email
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"📤 Enviando email a: {toEmail}");
                _logger.LogInformation($"   Asunto: {subject}");

                // Si la configuración está vacía, usa valores por defecto
                var effectiveSettings = GetEffectiveSettings();
                
                // Validaciones
                if (string.IsNullOrWhiteSpace(toEmail))
                    throw new ArgumentException("Email destino no puede estar vacío");

                // Crear mensaje
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(effectiveSettings.SenderEmail, effectiveSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };
                
                mailMessage.To.Add(toEmail);

                // Configurar cliente SMTP
                using var smtpClient = new SmtpClient(effectiveSettings.SmtpServer, effectiveSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(effectiveSettings.Username, effectiveSettings.Password),
                    EnableSsl = effectiveSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = effectiveSettings.Timeout,
                    UseDefaultCredentials = false
                };

                // Enviar email
                await smtpClient.SendMailAsync(mailMessage);
                
                _logger.LogInformation($"✅ Email enviado exitosamente a: {toEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"❌ Error SMTP enviando email a {toEmail}");
                
                // Errores específicos de Gmail
                switch (smtpEx.StatusCode)
                {
                    case SmtpStatusCode.GeneralFailure:
                        _logger.LogError("Error general de SMTP. Verifica conexión a internet.");
                        break;
                    case SmtpStatusCode.ClientNotPermitted:
                        _logger.LogError("Cliente no permitido. Usa App Password, no contraseña normal.");
                        break;
                    case SmtpStatusCode.MustIssueStartTlsFirst:
                        _logger.LogError("Requiere STARTTLS. Asegúrate que EnableSsl=true y puerto 587.");
                        break;
                }
                
                return false;
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, $"🔐 Error de autenticación con Gmail para {toEmail}");
                _logger.LogError("   Verifica: 1) App Password correcta 2) EnableSsl=true");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Error inesperado enviando email a {toEmail}");
                return false;
            }
        }

        // Método para obtener configuración efectiva (con valores por defecto si es necesario)
        private EmailConfiguration GetEffectiveSettings()
        {
            // Si la configuración está vacía, usa valores por defecto
            if (string.IsNullOrWhiteSpace(_settings.SmtpServer) || 
                string.IsNullOrWhiteSpace(_settings.Username) || 
                string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogWarning("⚠️ Configuración de email incompleta. Usando valores por defecto.");
                
                return new EmailConfiguration
                {
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    SenderEmail = "holanokia123@gmail.com",
                    SenderName = "Sistema Agrícola",
                    Username = "holanokia123@gmail.com",
                    Password = "yfouhdklkdhpswby", // Tu App Password
                    EnableSsl = true,
                    Timeout = 30000
                };
            }
            
            return _settings;
        }

        // Método específico para código de verificación (6 dígitos)
        public async Task<bool> SendVerificationCodeAsync(string toEmail, string code)
        {
            var subject = "✅ Código de verificación - Sistema Agrícola";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4CAF50;'>Verificación de Email</h2>
                    <p>Tu código de verificación es:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <div style='font-size: 36px; font-weight: bold; color: #2E7D32; letter-spacing: 10px;'>
                            {code}
                        </div>
                    </div>
                    <p>Este código es válido por <strong>10 minutos</strong>.</p>
                    <br>
                    <p>Saludos,<br>Equipo Sistema Agrícola</p>
                </div>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Método para email de bienvenida
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "👋 ¡Bienvenido/a al Sistema de Gestión Agrícola!";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h1 style='color: #2E7D32;'>¡Bienvenido/a, {userName}!</h1>
                    <p>Tu cuenta ha sido creada exitosamente en el Sistema de Gestión Agrícola.</p>
                    <br>
                    <p>Saludos,<br>Equipo Sistema Agrícola</p>
                </div>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Método para restablecer contraseña
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "🔐 Restablecer contraseña - Sistema Agrícola";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h1 style='color: #D32F2F;'>Restablecer contraseña</h1>
                    <p>Usa el siguiente código para restablecer tu contraseña:</p>
                    <div style='text-align: center; margin: 25px 0;'>
                        <div style='font-family: monospace; font-size: 24px; font-weight: bold; color: #d32f2f; letter-spacing: 3px;'>
                            {resetToken}
                        </div>
                    </div>
                    <p>Este código expira en <strong>24 horas</strong>.</p>
                    <br>
                    <p>Saludos,<br>Equipo Sistema Agrícola</p>
                </div>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Método adicional: Notificación del sistema
        public async Task<bool> SendSystemNotificationAsync(string toEmail, string title, string message, string notificationType = "info")
        {
            var icon = notificationType switch
            {
                "warning" => "⚠️",
                "error" => "❌",
                "success" => "✅",
                _ => "ℹ️"
            };

            var subject = $"{icon} {title} - Sistema Agrícola";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2>{icon} {title}</h2>
                    <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        {message}
                    </div>
                    <p>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}