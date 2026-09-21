namespace ClServApp.Server.Models
{
    public class OrderDetail
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
    }
}