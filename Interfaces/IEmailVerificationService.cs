using System.Threading.Tasks;

namespace SistemaGestionAgricola.Services
{
    public interface IEmailVerificationService
    {
        Task<string> GenerateAndSendVerificationCodeAsync(string email, string purpose = "register");
        Task<bool> VerifyCodeAsync(string email, string code);
        Task<bool> ResendVerificationCodeAsync(string email);
    }
}