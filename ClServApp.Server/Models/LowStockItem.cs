namespace ClServApp.Server.Models
{
    public class LowStockItem
    {
        public string Warehouse { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
    }
}