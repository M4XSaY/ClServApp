namespace ClServApp.Server.Models
{
    public class OrderCreate
    {
        public int ClientId { get; set; }
        public int EmployeeId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}