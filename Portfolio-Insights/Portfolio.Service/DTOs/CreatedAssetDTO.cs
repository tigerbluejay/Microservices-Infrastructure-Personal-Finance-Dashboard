namespace Portfolio.Service.DTOs
{
    // Request when creating an asset
    public record CreatedAssetDto(string Symbol, string Name, decimal Quantity);
}