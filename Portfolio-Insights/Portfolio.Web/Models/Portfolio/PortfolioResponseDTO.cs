namespace Portfolio.Web.Models.Portfolio;

public class PortfolioResponseDto
{
    public string UserName { get; set; } = string.Empty;
    public List<AssetDto> Assets { get; set; } = new();
    public decimal TotalValue { get; set; }
}