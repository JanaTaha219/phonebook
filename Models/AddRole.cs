using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class AddRole
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
    }
}
