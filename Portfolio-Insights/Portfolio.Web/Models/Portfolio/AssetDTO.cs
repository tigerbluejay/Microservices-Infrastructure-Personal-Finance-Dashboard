namespace Portfolio.Web.Models.Portfolio;

public class AssetDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Value { get; set; }
}