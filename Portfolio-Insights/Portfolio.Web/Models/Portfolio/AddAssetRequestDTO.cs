namespace Portfolio.Web.Models.Portfolio;

public class AddAssetRequestDto
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}