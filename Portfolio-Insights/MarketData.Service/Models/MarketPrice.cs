namespace MarketData.Service.Models
{
    public class MarketPrice
    {
        public string Symbol { get; set; } = default!;
        public decimal Price { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
