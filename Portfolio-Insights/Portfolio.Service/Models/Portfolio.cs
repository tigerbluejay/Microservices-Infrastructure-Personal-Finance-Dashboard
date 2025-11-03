namespace Portfolio.Service.Models
{
    public class Portfolio
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!;
        public List<PortfolioAsset> Assets { get; set; } = new();
    }
}
