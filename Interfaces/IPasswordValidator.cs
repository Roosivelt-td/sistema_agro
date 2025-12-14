namespace SistemaGestionAgricola.Services
{
    public interface IPasswordValidator
    {
        (bool IsValid, string Message) ValidatePassword(string password);
        string GetPasswordRequirements(); 
    }
}