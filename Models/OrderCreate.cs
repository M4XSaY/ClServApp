namespace ClServApp.Server.Models
{
    public class OrderCreate
    {
        public int ClientId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; } = "Новый";
        public List<OrderItemCreate> Items { get; set; } = new();
    }
}
