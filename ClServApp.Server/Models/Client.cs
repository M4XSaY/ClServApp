namespace ClServApp.Server.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Inn { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
    }
}