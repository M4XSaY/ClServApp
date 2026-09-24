namespace ClServApp.Server.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public DateTime HireDate { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;
    }
}