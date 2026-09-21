namespace ClServApp.Server.Models
{
    public class StockItem
    {
        public string Warehouse { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string AlcoholType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
    }
}