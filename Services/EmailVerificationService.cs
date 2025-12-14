using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SistemaGestionAgricola.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailVerificationService> _logger;
        
        private const int CODE_LENGTH = 6;
        private const int EXPIRATION_MINUTES = 10;

        public EmailVerificationService(
            IMemoryCache cache,
            IEmailService emailService,
            ILogger<EmailVerificationService> logger)
        {
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateAndSendVerificationCodeAsync(string email, string purpose = "register")
        {
            try
            {
                _logger.LogInformation($"Generando código de verificación para {email}");
                
                // Generar código de 6 dígitos
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();
                
                // Guardar en cache con expiración
                var cacheKey = $"VerifyCode:{purpose}:{email.ToLower()}";
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(EXPIRATION_MINUTES));
                
                _cache.Set(cacheKey, code, cacheEntryOptions);
                
                _logger.LogInformation($"Código {code} generado para {email} (expira en {EXPIRATION_MINUTES} min)");
                
                // Enviar email con el código usando IEmailService
                var emailSent = await _emailService.SendVerificationCodeAsync(email, code);
                
                if (!emailSent)
                {
                    _logger.LogError($"No se pudo enviar email de verificación a {email}");
                    // No lanzar excepción, solo devolver null
                    return null;
                }
                
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generando código para {email}");
                return null;
            }
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                {
                    _logger.LogWarning($"Email o código vacío para verificación");
                    return false;
                }

                // Intentar verificar para registro
                var registerCacheKey = $"VerifyCode:register:{email.ToLower()}";
                if (_cache.TryGetValue(registerCacheKey, out string cachedCode) && cachedCode == code)
                {
                    _cache.Remove(registerCacheKey);
                    _logger.LogInformation($"✅ Código verificado para registro: {email}");
                    return true;
                }

                // Intentar verificar para reset de contraseña
                var resetCacheKey = $"VerifyCode:reset:{email.ToLower()}";
                if (_cache.TryGetValue(resetCacheKey, out string resetCachedCode) && resetCachedCode == code)
                {
                    _cache.Remove(resetCacheKey);
                    _logger.LogInformation($"✅ Código verificado para reset: {email}");
                    return true;
                }

                // Código general (fallback)
                var generalCacheKey = $"VerifyCode:general:{email.ToLower()}";
                if (_cache.TryGetValue(generalCacheKey, out string generalCachedCode) && generalCachedCode == code)
                {
                    _cache.Remove(generalCacheKey);
                    _logger.LogInformation($"✅ Código general verificado: {email}");
                    return true;
                }

                _logger.LogWarning($"❌ Código inválido o expirado para {email}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando código para {email}");
                return false;
            }
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Reenviando código de verificación a {email}");
                
                // Intentar obtener código existente primero
                var cacheKey = $"VerifyCode:register:{email.ToLower()}";
                if (_cache.TryGetValue(cacheKey, out string existingCode))
                {
                    // Reenviar el mismo código
                    var emailSent = await _emailService.SendVerificationCodeAsync(email, existingCode);
                    
                    if (emailSent)
                    {
                        _logger.LogInformation($"✅ Código reenviado a {email}");
                        return true;
                    }
                }
                
                // Si no hay código existente o falló, generar uno nuevo
                var newCode = await GenerateAndSendVerificationCodeAsync(email, "register");
                return !string.IsNullOrEmpty(newCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reenviando código a {email}");
                return false;
            }
        }
    }
}