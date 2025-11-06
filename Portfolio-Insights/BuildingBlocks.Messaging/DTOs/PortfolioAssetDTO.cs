namespace BuildingBlocks.Messaging.DTOs;

public record PortfolioAssetDto
{
    public string Symbol { get; init; } = default!;
    public decimal Quantity { get; init; }

    // Optional: include Name if you want to use it internally
    public string? Name { get; init; }

    public PortfolioAssetDto() { }

    public PortfolioAssetDto(string symbol, decimal quantity, string? name = null)
    {
        Symbol = symbol;
        Quantity = quantity;
        Name = name;
    }
}