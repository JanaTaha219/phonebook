namespace WebApplication8.Models
{
    public class ImportantNumber
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public ImportantNumber() { }

        public ImportantNumber(int employeeId)
        {
            EmployeeId = employeeId;
        }

    }

}
