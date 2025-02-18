using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Email { get; set; }
        [Required]
        public long LocalPhone { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? OfficeLocation { get; set; }
        public string? Notes { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OtherNumbers { get; set; }

        public int counter { get; set; } = 0;

        public void IncrementCounter()
        {
            counter++;
        }

        public List<SearchHistory>? SearchHistories { get; set; }

    }
}
