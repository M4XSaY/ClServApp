namespace ClServApp.Server.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AlcoholType { get; set; } = string.Empty;
        public decimal Strength { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}