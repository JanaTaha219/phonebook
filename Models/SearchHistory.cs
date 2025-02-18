using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class SearchHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }  // Link to Employee (foreign key)

        public DateTime SearchDate { get; set; } = DateTime.Now;


        // Navigation property
        public Employee Employee { get; set; }
    }

}
