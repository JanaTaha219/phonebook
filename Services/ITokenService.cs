namespace WebApplication8.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId);
        bool ValidateToken(string token, out string userId);
        public void MarkTokenAsUsed(string token);
    }
}
