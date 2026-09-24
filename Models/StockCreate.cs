namespace ClServApp.Server.Models
{
    public class StockCreate
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
    }
}
