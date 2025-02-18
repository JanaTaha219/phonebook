using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class TokenEntity
    {
        [Key]
        [StringLength(450)]
        public string Token { get; set; }
        public string UserId { get; set; }
        public bool Used { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
