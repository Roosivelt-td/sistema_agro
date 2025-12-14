using System.IdentityModel.Tokens.Jwt;

namespace SistemaGestionAgricola.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo para endpoints de autenticación
            if (context.Request.Path.StartsWithSegments("/api/Auth"))
            {
                await _next(context);
                return;
            }
            
            Console.WriteLine($"\n=== REQUEST {DateTime.Now:HH:mm:ss} ===");
            Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
            
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                Console.WriteLine("⚠️ NO hay Authorization header");
            }
            else
            {
                Console.WriteLine($"Authorization: {authHeader}");
                
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    Console.WriteLine($"Token recibido: {token.Substring(0, Math.Min(30, token.Length))}...");
                    
                    try
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        if (tokenHandler.CanReadToken(token))
                        {
                            var jwtToken = tokenHandler.ReadJwtToken(token);
                            Console.WriteLine($"✅ Token válido - Claims:");
                            foreach (var claim in jwtToken.Claims)
                            {
                                Console.WriteLine($"  {claim.Type}: {claim.Value}");
                            }
                            Console.WriteLine($"Expira: {jwtToken.ValidTo.ToLocalTime()}");
                        }
                        else
                        {
                            Console.WriteLine("❌ Token NO puede leerse");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error leyendo token: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Header no empieza con 'Bearer'");
                }
            }
            
            await _next(context);
            
            Console.WriteLine($"Response Status: {context.Response.StatusCode}");
            Console.WriteLine("=== END REQUEST ===\n");
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}