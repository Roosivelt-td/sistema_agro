using Microsoft.EntityFrameworkCore;
using SistemaGestionAgricola.Data;
//using SistemaGestionAgricola.Extensions;
using SistemaGestionAgricola.Helpers;
using SistemaGestionAgricola.Middleware;
using SistemaGestionAgricola.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddCustomJsonOptions();

// Configurar CORS
builder.Services.AddCustomCors();

// Configurar JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add DbContext - USANDO MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' no configurada");
}

// Asegúrate de tener: dotnet add package Pomelo.EntityFrameworkCore.MySql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

// Configurar servicios de aplicación
builder.Services.AddApplicationServices(builder.Configuration);

// Agregar logging mejorado para emails
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add Swagger CON AUTORIZACIÓN JWT
builder.Services.AddCustomSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Servir archivos estáticos (para el JS personalizado)
    app.UseStaticFiles(); 

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Gestión Agrícola API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:5173/
        c.DisplayOperationId();
        c.DisplayRequestDuration();
        
        // Configuración adicional para facilitar las pruebas
        c.DefaultModelsExpandDepth(-1); // Oculta el panel de schemas por defecto
        c.EnableFilter(); // Habilita filtro de búsqueda
        c.ShowExtensions();
        // AGREGAR ESTO para JavaScript personalizado
        c.InjectJavascript("/swagger/custom.js");
    });
    
    // Aplicar migraciones automáticamente
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("✅ Base de datos migrada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al migrar base de datos: {ex.Message}");
        }
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // IMPORTANTE: Primero Authentication
app.UseAuthorization();   // IMPORTANTE: Luego Authorization
app.MapControllers();

// Middleware para logging de requests (opcional)
app.UseRequestLogging();

Console.WriteLine("🚀 Aplicación iniciada en: " + (app.Environment.IsDevelopment() ? "http://localhost:5173" : "Producción"));
Console.WriteLine("📚 Swagger disponible en: http://localhost:5173");
Console.WriteLine("🔐 Recuerda usar el botón 'Authorize' en Swagger para probar endpoints protegidos");

app.Run();
