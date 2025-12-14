// 📁 Services/IJwtService.cs
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Services
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
    }
}