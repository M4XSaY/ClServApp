namespace ClServApp.Server.Models
{
    public class OrderEdit
    {
        public int ClientId { get; set; }
        public int EmployeeId { get; set; }
        public List<OrderItemCreate> Items { get; set; } = new();
    }
}