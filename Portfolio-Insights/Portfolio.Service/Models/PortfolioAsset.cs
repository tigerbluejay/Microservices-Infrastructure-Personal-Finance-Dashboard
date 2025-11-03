namespace Portfolio.Service.Models
{
    public class PortfolioAsset
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = default!;   // e.g. AAPL
        public string Name { get; set; } = default!;
        public decimal Quantity { get; set; }
    }
}
