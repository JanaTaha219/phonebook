using System.Text.Json;
using StackExchange.Redis;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Services
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TokenService> _logger;

        public TokenService(ApplicationDbContext context, ILogger<TokenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GenerateToken(string userId)
        {
            var token = Guid.NewGuid().ToString();
            var tokenEntity = new TokenEntity
            {
                Token = token,
                UserId = userId,
                Used = false,
                ExpirationTime = DateTime.UtcNow.AddHours(24)
            };

            _context.Tokens.Add(tokenEntity);
            _context.SaveChanges();
            return token;
        }

        public bool ValidateToken(string token, out string userId)
        {
            userId = null;
            try
            {
                var tokenEntity = _context.Tokens
                    .FirstOrDefault(t => t.Token == token
                        && !t.Used
                        && t.ExpirationTime > DateTime.UtcNow);

                if (tokenEntity == null) return false;

                userId = tokenEntity.UserId;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        public void MarkTokenAsUsed(string token)
        {
            try
            {
                var tokenEntity = _context.Tokens.Find(token);
                if (tokenEntity != null)
                {
                    _context.Tokens.Remove(tokenEntity);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking token as used");
            }
        }
    }
}
