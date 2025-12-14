using SistemaGestionAgricola.Services;

namespace SistemaGestionAgricola.Helpers
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicios de aplicación
            services.AddScoped<IJwtService, JwtService>();
            
            // Agrega MemoryCache
            services.AddMemoryCache();

            // ⭐⭐⭐⭐ CONFIGURACIÓN CRÍTICA DE EMAIL ⭐⭐⭐⭐
            // Esto debe ser EmailConfiguration (NO EmailSettings)
            services.Configure<EmailConfiguration>(
                configuration.GetSection("EmailSettings"));

            // Servicios de Email
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();

            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IPasswordValidator, PasswordValidator>(); // Contraseña Segura

            return services;
        }
    }
}