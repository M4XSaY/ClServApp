namespace ClServApp.Server.Models
{
    public class EmployeeCreate
    {
        public string Name { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public DateTime HireDate { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }
    }
}