using Microsoft.OpenApi.Models;

namespace SistemaGestionAgricola.Helpers
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Sistema Gestión Agrícola API", 
                    Version = "v1",
                    Description = "API para el sistema de gestión agrícola",
                    Contact = new OpenApiContact
                    {
                        Name = "Soporte",
                        Email = "soporte@sistemaagricola.com"
                    }
                });
                
                // CONFIGURACIÓN DE AUTORIZACIÓN JWT EN SWAGGER
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresa el token JWT. Ejemplo: Bearer eyJhbGciOi..."
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                
                // Esto ayuda a mostrar mejor los enums
                c.UseAllOfToExtendReferenceSchemas();
                
                // Opcional: Ordenar los endpoints por nombre
                c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
            });

            return services;
        }
    }
}