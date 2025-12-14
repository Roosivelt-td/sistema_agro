using Microsoft.AspNetCore.Mvc;
using SistemaGestionAgricola.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SistemaGestionAgricola.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestController> _logger;
        private readonly IConfiguration _configuration;

        public TestController(
            IEmailService emailService, 
            ILogger<TestController> logger,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        // 1. Endpoint que usa IEmailService (configuración desde appsettings)
        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation($"📧 Solicitando email de prueba a {request.Email}");
                
                var subject = "¡Prueba de email exitosa! - Sistema Agrícola";
                var body = $@"
                    <h1>¡Hola!</h1>
                    <p>Este es un <strong>email de prueba</strong> desde tu Sistema de Gestión Agrícola.</p>
                    <p>Fecha: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p>
                    <p style='color: green; font-weight: bold;'>✅ Configuración exitosa</p>
                    <p>Saludos,<br>El equipo de desarrollo</p>
                ";

                var emailSent = await _emailService.SendEmailAsync(request.Email, subject, body);

                if (emailSent)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"✅ Email enviado a {request.Email}",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "❌ Error al enviar email",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de email");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        // 2. Endpoint hardcodeado CORREGIDO (usa misma App Password)
        [HttpPost("send-test-email-hardcoded")]
        public async Task<IActionResult> SendTestEmailHardcoded([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation($"📧 Enviando email HARDCODEADO CORREGIDO a {request.Email}");
                
                // ⚠️ USA LOS MISMOS VALORES QUE FUNCIONAN
                var smtpServer = "smtp.gmail.com";
                var smtpPort = 587;
                var senderEmail = "lasireick@gmail.com";
                var username = "lasireick@gmail.com";
                var password = "yfouhdklkdhpswby"; // ← MISMA APP PASSWORD QUE FUNCIONA
                
                var subject = "¡Prueba Hardcoded Corregida! - Sistema Agrícola";
                var body = $@"
                    <h1>¡Hola!</h1>
                    <p>Este email usa los <strong>valores hardcodeados corregidos</strong>.</p>
                    <p>Fecha: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p>
                    <p>Si recibes esto, ¡todos los endpoints funcionan!</p>
                ";
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Sistema Agrícola"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(request.Email);

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                    Timeout = 30000
                };

                await smtpClient.SendMailAsync(mailMessage);
                
                return Ok(new
                {
                    success = true,
                    message = $"✅ Email HARDCODEADO CORREGIDO enviado a {request.Email}",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en email hardcodeado");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }

        // 3. Endpoint específico para App Password (ya funciona)
        [HttpPost("send-test-gmail-apppassword")]
        public async Task<IActionResult> SendTestGmailAppPassword([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation($"📧 Enviando email con APP PASSWORD GMAIL a {request.Email}");
                
                var smtpServer = "smtp.gmail.com";
                var smtpPort = 587;
                var senderEmail = "lasireick@gmail.com";
                var username = "lasireick@gmail.com";
                var password = "yfouhdklkdhpswby";
                
                _logger.LogInformation($"   Usando App Password (primeros 4 chars): {password.Substring(0, 4)}...");
                
                var subject = "✅ ¡Prueba Gmail App Password! - Sistema Agrícola";
                var body = $@"
                    <div style='font-family: Arial; padding: 20px;'>
                        <h1 style='color: #EA4335;'>¡Prueba Gmail!</h1>
                        <p>Este email fue enviado usando <strong>Contraseña de Aplicación</strong>.</p>
                        <p>Fecha: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p>
                        <p>Si recibes esto, ¡la configuración funciona!</p>
                        <!-- ⚠️ NO muestres la contraseña en el email -->
                    </div>
                ";
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Sistema Agrícola"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(request.Email);

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                    Timeout = 30000
                };

                await smtpClient.SendMailAsync(mailMessage);
                
                return Ok(new
                {
                    success = true,
                    message = $"✅ Email enviado via GMAIL APP PASSWORD a {request.Email}",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error con Gmail App Password");
                
                var errorMessage = ex.Message;
                var solution = "";
                
                if (errorMessage.Contains("5.7.0"))
                {
                    solution = "1. Gmail puede estar bloqueando contenido sensible\n" +
                              "2. No muestres contraseñas en el cuerpo del email\n" +
                              "3. Revisa si el email está en SPAM\n" +
                              "4. Intenta con contenido más genérico";
                }
                
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {errorMessage}",
                    solution = solution
                });
            }
        }

        // 4. Endpoint para debug de configuración
        [HttpGet("debug-email-config")]
        public IActionResult DebugEmailConfig()
        {
            try
            {
                // Obtiene la configuración directamente
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration["EmailSettings:SmtpPort"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = _configuration["EmailSettings:EnableSsl"];
                
                return Ok(new
                {
                    success = true,
                    config = new
                    {
                        SmtpServer = smtpServer ?? "NULL",
                        SmtpPort = smtpPort ?? "NULL",
                        SenderEmail = senderEmail ?? "NULL",
                        SenderName = senderName ?? "NULL",
                        Username = username ?? "NULL",
                        Password = string.IsNullOrEmpty(password) ? "NULL o vacío" : "*** Configurada ***",
                        EnableSsl = enableSsl ?? "NULL"
                    },
                    allSections = _configuration.GetSection("EmailSettings").GetChildren()
                        .Select(x => new { Key = x.Key, Value = x.Value ?? "NULL" })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error leyendo configuración: {ex.Message}",
                    details = ex.StackTrace
                });
            }
        }

        // 5. Endpoint simple para Mailtrap (alternativa)
        [HttpPost("send-test-mailtrap")]
        public async Task<IActionResult> SendTestMailtrap([FromBody] TestEmailRequest request)
        {
            try
            {
                // Configuración SIMPLE de Mailtrap
                using var client = new SmtpClient("sandbox.smtp.mailtrap.io", 587)
                {
                    Credentials = new NetworkCredential("436c121c782f33", "ca077267761513"),
                    EnableSsl = true,
                    Timeout = 30000
                };
                
                await client.SendMailAsync(
                    new MailMessage("no-reply@sistema.test", request.Email)
                    {
                        Subject = "✅ Mailtrap Test - Sistema Agrícola",
                        Body = $"<h1>Mailtrap funciona</h1><p>{DateTime.Now}</p>",
                        IsBodyHtml = true
                    }
                );
                
                return Ok(new { 
                    success = true, 
                    message = "✅ Email enviado con Mailtrap",
                    check = "Revisa: https://mailtrap.io/inboxes"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    solution = "1. Verifica credenciales Mailtrap 2. Verifica conexión internet"
                });
            }
        }

        public class TestEmailRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }
    }
}