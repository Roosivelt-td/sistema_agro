using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionAgricola.Data;
using SistemaGestionAgricola.Models.Entities;
using SistemaGestionAgricola.Models.DTOs;
using SistemaGestionAgricola.Services;
using Microsoft.Extensions.Logging;

namespace SistemaGestionAgricola.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IConfiguration _configuration;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            IJwtService jwtService, 
            IPasswordService passwordService,
            IPasswordValidator passwordValidator,
            IConfiguration configuration,
            IEmailVerificationService emailVerificationService,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _passwordValidator = passwordValidator;
            _configuration = configuration;
            _emailVerificationService = emailVerificationService;
            _emailService = emailService;
            _logger = logger;
        }

        // ==================== LOGIN ====================
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(loginDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (string.IsNullOrWhiteSpace(loginDTO.Password))
                    return BadRequest(new { 
                        success = false, 
                        message = "La contraseña es requerida" 
                    });

                // Validar formato de email
                if (!IsValidEmail(loginDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

                if (usuario == null)
                    return Unauthorized(new { 
                        success = false, 
                        message = "Credenciales inválidas. Verifica tu email y contraseña." 
                    });

                // ✅ VERIFICAR SI EL EMAIL ESTÁ VERIFICADO
                if (!usuario.IsEmailVerified)
                {
                    // Opcional: Reenviar código automáticamente
                    try
                    {
                        await _emailVerificationService.ResendVerificationCodeAsync(usuario.Email);
                        
                        return Unauthorized(new { 
                            success = false, 
                            message = "Debes verificar tu email antes de iniciar sesión. Hemos reenviado el código de verificación.",
                            requiresEmailVerification = true,
                            email = usuario.Email,
                            canResend = true
                        });
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Error reenviando código para {usuario.Email}");
                        
                        return Unauthorized(new { 
                            success = false, 
                            message = "Tu email no está verificado. Por favor verifica tu email antes de iniciar sesión.",
                            requiresEmailVerification = true,
                            email = usuario.Email
                        });
                    }
                }

                // Verificar contraseña
                if (!_passwordService.VerifyPassword(loginDTO.Password, usuario.PasswordHash))
                {
                    _logger.LogWarning($"Intento de login fallido para {loginDTO.Email}");
                    
                    return Unauthorized(new { 
                        success = false, 
                        message = "Credenciales inválidas. Verifica tu email y contraseña." 
                    });
                }

                // Generar token
                var token = _jwtService.GenerateToken(usuario);

                var response = new AuthResponseDTO
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = new UsuarioDTO
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        Rol = usuario.Rol,
                        Nombre = usuario.Nombre,
                        Apellidos = usuario.Apellidos ?? string.Empty,
                        Telefono = usuario.Telefono,
                        CreatedAt = usuario.CreatedAt,
                        UpdatedAt = usuario.UpdatedAt,
                        IsEmailVerified = usuario.IsEmailVerified
                    }
                };

                _logger.LogInformation($"✅ Login exitoso para usuario: {usuario.Email}");
                
                return Ok(new {
                    success = true,
                    message = "✅ Inicio de sesión exitoso",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en login para email: {loginDTO.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== REGISTRO - PRIMERO EMAIL ====================
        [HttpPost("register/email")]
        public async Task<IActionResult> RegisterEmail([FromBody] RegisterEmailDTO registerEmailDTO)
        {
            try
            {
                // Validar campo requerido
                if (string.IsNullOrWhiteSpace(registerEmailDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });
                
                // Validar formato de email
                if (!IsValidEmail(registerEmailDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Validar que el email no exista
                if (await _context.Usuarios.AnyAsync(u => u.Email == registerEmailDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email ya está registrado" 
                    });

                // Enviar código de verificación sin crear usuario aún
                try
                {
                    var code = await _emailVerificationService.GenerateAndSendVerificationCodeAsync(
                        registerEmailDTO.Email, 
                        "register"
                    );

                    if (string.IsNullOrEmpty(code))
                    {
                        return StatusCode(500, new { 
                            success = false, 
                            message = "Error enviando código de verificación. Por favor intenta nuevamente." 
                        });
                    }

                    _logger.LogInformation($"✅ Código de verificación enviado a {registerEmailDTO.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Error enviando email de verificación a {registerEmailDTO.Email}");
                    
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Error enviando código de verificación. Por favor intenta nuevamente." 
                    });
                }

                return Ok(new {
                    success = true,
                    message = "✅ Código de verificación enviado. Por favor revisa tu email.",
                    email = registerEmailDTO.Email,
                    canCompleteRegistration = true,
                    expiresInMinutes = 10
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en registro de email: {registerEmailDTO.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== COMPLETAR REGISTRO ====================
        [HttpPost("register/complete")]
        public async Task<ActionResult<AuthResponseDTO>> CompleteRegister([FromBody] CompleteRegisterDTO completeRegisterDTO)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(completeRegisterDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (string.IsNullOrWhiteSpace(completeRegisterDTO.Code))
                    return BadRequest(new { 
                        success = false, 
                        message = "El código de verificación es requerido" 
                    });

                if (string.IsNullOrWhiteSpace(completeRegisterDTO.Password))
                    return BadRequest(new { 
                        success = false, 
                        message = "La contraseña es requerida" 
                    });

                if (string.IsNullOrWhiteSpace(completeRegisterDTO.Nombre))
                    return BadRequest(new { 
                        success = false, 
                        message = "El nombre es requerido" 
                    });

                // Validar formato de email
                if (!IsValidEmail(completeRegisterDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Validar rol
                if (!IsValidRol(completeRegisterDTO.Rol))
                    return BadRequest(new { 
                        success = false, 
                        message = $"Rol '{completeRegisterDTO.Rol}' no válido. Roles permitidos: admin, agricultor, supervisor" 
                    });

                // Validar contraseña segura
                var passwordValidation = _passwordValidator.ValidatePassword(completeRegisterDTO.Password);
                if (!passwordValidation.IsValid)
                    return BadRequest(new { 
                        success = false, 
                        message = passwordValidation.Message 
                    });

                // Validar que el código sea correcto
                var isValidCode = await _emailVerificationService.VerifyCodeAsync(
                    completeRegisterDTO.Email, 
                    completeRegisterDTO.Code
                );

                if (!isValidCode)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Código de verificación inválido o expirado" 
                    });
                }

                // Validar que el email no exista (por si acaso)
                if (await _context.Usuarios.AnyAsync(u => u.Email == completeRegisterDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email ya está registrado" 
                    });

                // Crear usuario con datos completos
                var usuario = new Usuario
                {
                    Email = completeRegisterDTO.Email.Trim(),
                    PasswordHash = _passwordService.HashPassword(completeRegisterDTO.Password),
                    Rol = completeRegisterDTO.Rol.Trim(),
                    Nombre = completeRegisterDTO.Nombre.Trim(),
                    Apellidos = completeRegisterDTO.Apellidos?.Trim() ?? string.Empty,
                    Telefono = completeRegisterDTO.Telefono?.Trim(),
                    IsEmailVerified = true // ← AHORA SÍ: Email verificado
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Generar token ya que el email está verificado
                var token = _jwtService.GenerateToken(usuario);

                var response = new AuthResponseDTO
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = new UsuarioDTO
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        Rol = usuario.Rol,
                        Nombre = usuario.Nombre,
                        Apellidos = usuario.Apellidos ?? string.Empty,
                        Telefono = usuario.Telefono,
                        CreatedAt = usuario.CreatedAt,
                        UpdatedAt = usuario.UpdatedAt,
                        IsEmailVerified = usuario.IsEmailVerified
                    }
                };

                _logger.LogInformation($"✅ Registro completo exitoso para {usuario.Email}");

                return Ok(new {
                    success = true,
                    message = "✅ Registro completado exitosamente",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en registro completo: {completeRegisterDTO.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== REGISTRO TRADICIONAL (DEPRECATED) ====================
        [HttpPost("register")]
        [Obsolete("Use register/email and register/complete endpoints instead")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDTO)
        {
            try
            {
                // Advertencia de deprecación
                _logger.LogWarning($"Uso de endpoint obsoleto: register para {registerDTO.Email}. Use register/email y register/complete en su lugar.");
                
                // Validar que el email no exista
                if (await _context.Usuarios.AnyAsync(u => u.Email == registerDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email ya está registrado" 
                    });
                
                // Validar formato de email
                if (!IsValidEmail(registerDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Validar rol
                if (!IsValidRol(registerDTO.Rol))
                    return BadRequest(new { 
                        success = false, 
                        message = $"Rol '{registerDTO.Rol}' no válido. Roles permitidos: admin, agricultor, supervisor" 
                    });

                // VALIDACIÓN DE CONTRASEÑA SEGURA
                var passwordValidation = _passwordValidator.ValidatePassword(registerDTO.Password);
                if (!passwordValidation.IsValid)
                    return BadRequest(new { 
                        success = false, 
                        message = passwordValidation.Message 
                    });
                
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(registerDTO.Nombre))
                    return BadRequest(new { 
                        success = false, 
                        message = "El nombre es requerido" 
                    });

                // Crear usuario CON HASH (pero NO verificado aún)
                var usuario = new Usuario
                {
                    Email = registerDTO.Email.Trim(),
                    PasswordHash = _passwordService.HashPassword(registerDTO.Password),
                    Rol = registerDTO.Rol.Trim(),
                    Nombre = registerDTO.Nombre.Trim(),
                    Apellidos = registerDTO.Apellidos?.Trim() ?? string.Empty,
                    Telefono = registerDTO.Telefono?.Trim(),
                    IsEmailVerified = false // ← IMPORTANTE: No verificado inicialmente
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // ✅ ENVIAR CÓDIGO DE VERIFICACIÓN POR EMAIL
                try
                {
                    var code = await _emailVerificationService.GenerateAndSendVerificationCodeAsync(
                        usuario.Email, 
                        "register"
                    );

                    if (string.IsNullOrEmpty(code))
                    {
                        // Eliminar usuario si no se pudo enviar el email
                        _context.Usuarios.Remove(usuario);
                        await _context.SaveChangesAsync();
                        
                        return StatusCode(500, new { 
                            success = false, 
                            message = "Error enviando código de verificación. Por favor intenta nuevamente." 
                        });
                    }

                    _logger.LogInformation($"✅ Código de verificación enviado a {usuario.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Error enviando email de verificación a {usuario.Email}");
                    
                    // Eliminar usuario si no se pudo enviar el email
                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();
                    
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Error enviando código de verificación. Por favor intenta nuevamente." 
                    });
                }

                // NO generar token aún - el usuario necesita verificar email primero
                return Ok(new {
                    success = true,
                    message = "✅ Usuario registrado exitosamente. Por favor verifica tu email con el código enviado. (Este endpoint será deprecado, use register/email y register/complete)",
                    requiresEmailVerification = true,
                    userId = usuario.Id,
                    email = usuario.Email,
                    canResend = true,
                    expiresInMinutes = 10
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en registro para email: {registerDTO.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== VERIFICAR EMAIL ====================
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6)
                    return BadRequest(new { 
                        success = false, 
                        message = "El código debe tener 6 dígitos" 
                    });

                // Verificar código
                var isValid = await _emailVerificationService.VerifyCodeAsync(
                    request.Email, 
                    request.Code
                );

                if (!isValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Código inválido, expirado o ya utilizado. Intenta nuevamente." 
                    });
                }

                // Buscar usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "Usuario no encontrado" 
                    });
                }

                // Marcar email como verificado
                usuario.IsEmailVerified = true;
                usuario.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Enviar email de bienvenida
                try
                {
                    await _emailService.SendWelcomeEmailAsync(usuario.Email, usuario.Nombre);
                    _logger.LogInformation($"✅ Email de bienvenida enviado a {usuario.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, $"No se pudo enviar email de bienvenida a {usuario.Email}");
                    // Continuar aunque falle el email de bienvenida
                }

                // Generar token ahora que el email está verificado
                var token = _jwtService.GenerateToken(usuario);

                var response = new AuthResponseDTO
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                    Usuario = new UsuarioDTO
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        Rol = usuario.Rol,
                        Nombre = usuario.Nombre,
                        Apellidos = usuario.Apellidos ?? string.Empty,
                        Telefono = usuario.Telefono,
                        CreatedAt = usuario.CreatedAt,
                        UpdatedAt = usuario.UpdatedAt,
                        IsEmailVerified = usuario.IsEmailVerified
                    }
                };

                _logger.LogInformation($"✅ Email verificado exitosamente para {usuario.Email}");

                return Ok(new {
                    success = true,
                    message = "✅ Email verificado exitosamente. ¡Bienvenido!",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando email: {request.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== REENVIAR VERIFICACIÓN ====================
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
        {
            try
            {
                // Validar email
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (!IsValidEmail(request.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    // Por seguridad, no revelar si el email existe o no
                    return Ok(new { 
                        success = true, 
                        message = "Si el email existe en nuestro sistema, recibirás un código de verificación." 
                    });
                }

                // Verificar si ya está verificado
                if (usuario.IsEmailVerified)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "El email ya está verificado" 
                    });
                }

                // Reenviar código
                var success = await _emailVerificationService.ResendVerificationCodeAsync(request.Email);

                if (!success)
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Error reenviando código de verificación. Por favor intenta más tarde." 
                    });
                }

                _logger.LogInformation($"✅ Código reenviado a {request.Email}");

                return Ok(new {
                    success = true,
                    message = "✅ Código de verificación reenviado. Revisa tu email.",
                    expiresInMinutes = 10
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reenviando verificación a: {request.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== OLVIDÉ CONTRASEÑA ====================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            try
            {
                // Validar email
                if (string.IsNullOrWhiteSpace(forgotPasswordDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (!IsValidEmail(forgotPasswordDTO.Email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == forgotPasswordDTO.Email);

                if (usuario == null)
                {
                    // Por seguridad, no revelar si el email existe
                    return Ok(new { 
                        success = true, 
                        message = "Si el email existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña." 
                    });
                }

                // Generar token de restablecimiento
                var resetToken = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("x");
                
                // GUARDAR TOKEN EN LA BASE DE DATOS
                usuario.ResetPasswordToken = resetToken;
                usuario.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24); // Expira en 24 horas
                usuario.UpdatedAt = DateTime.UtcNow;
                
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                
                // Enviar email con token
                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    usuario.Email, 
                    resetToken
                );

                if (!emailSent)
                {
                    // IMPORTANTE: Limpiar el token si falla el email
                    usuario.ResetPasswordToken = null;
                    usuario.ResetPasswordTokenExpiry = null;
                    await _context.SaveChangesAsync();
                    
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Error enviando email de restablecimiento" 
                    });
                }

                _logger.LogInformation($"✅ Email de restablecimiento enviado a {usuario.Email}");

                return Ok(new {
                    success = true,
                    message = "✅ Se han enviado instrucciones para restablecer tu contraseña a tu email."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en forgot-password para: {forgotPasswordDTO.Email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== RESTABLECER CONTRASEÑA ====================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(resetPasswordDTO.Token))
                    return BadRequest(new { success = false, message = "El token es requerido" });

                if (string.IsNullOrWhiteSpace(resetPasswordDTO.NewPassword))
                    return BadRequest(new { success = false, message = "La nueva contraseña es requerida" });

                if (resetPasswordDTO.NewPassword != resetPasswordDTO.ConfirmPassword)
                    return BadRequest(new { success = false, message = "Las contraseñas no coinciden" });

                // Validar contraseña segura
                var passwordValidation = _passwordValidator.ValidatePassword(resetPasswordDTO.NewPassword);
                if (!passwordValidation.IsValid)
                    return BadRequest(new { success = false, message = passwordValidation.Message });

                // 1. BUSCAR USUARIO POR TOKEN VÁLIDO
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.ResetPasswordToken == resetPasswordDTO.Token && 
                                             u.ResetPasswordTokenExpiry > DateTime.UtcNow);

                if (usuario == null)
                    return BadRequest(new { 
                        success = false, 
                        message = "Token inválido o expirado. Solicita un nuevo enlace." 
                    });

                // 2. HASHEAR LA NUEVA CONTRASEÑA
                string hashedPassword = _passwordService.HashPassword(resetPasswordDTO.NewPassword);

                // 3. ACTUALIZAR CONTRASEÑA Y LIMPIAR TOKEN
                usuario.PasswordHash = hashedPassword;
                usuario.ResetPasswordToken = null;
                usuario.ResetPasswordTokenExpiry = null;
                usuario.UpdatedAt = DateTime.UtcNow;

                // 4. GUARDAR CAMBIOS EN BD
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Contraseña restablecida para usuario ID: {usuario.Id}");

                return Ok(new {
                    success = true,
                    message = "✅ Contraseña restablecida exitosamente. Ahora puedes iniciar sesión con tu nueva contraseña."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en reset-password");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error interno del servidor" 
                });
            }
        }

        // ==================== VERIFICAR EMAIL DISPONIBLE ====================
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El email es requerido" 
                    });

                if (!IsValidEmail(email))
                    return BadRequest(new { 
                        success = false, 
                        message = "El formato del email no es válido" 
                    });

                var exists = await _context.Usuarios.AnyAsync(u => u.Email == email);

                return Ok(new {
                    success = true,
                    email = email,
                    available = !exists,
                    message = exists ? "El email ya está registrado" : "El email está disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando email: {email}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== REQUISITOS DE CONTRASEÑA ====================
        [HttpGet("password-requirements")]
        public IActionResult GetPasswordRequirements()
        {
            try
            {
                var requirements = _passwordValidator.GetPasswordRequirements();
                
                return Ok(new {
                    success = true,
                    requirements = requirements
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo requisitos de contraseña");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Error interno del servidor: {ex.Message}" 
                });
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================
        private bool IsValidRol(string rol)
        {
            var rolesPermitidos = new[] { "admin", "agricultor", "supervisor" };
            return rolesPermitidos.Contains(rol?.ToLower());
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}