// 📁 Services/IPasswordService.cs
namespace SistemaGestionAgricola.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}