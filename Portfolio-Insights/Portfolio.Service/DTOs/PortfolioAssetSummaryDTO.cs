namespace Portfolio.Service.DTOs
{
    public record CreateOrUpdatePortfolioResponse(string UserName, List<PortfolioAssetSummaryDto> Assets);

    public record PortfolioAssetSummaryDto(string Symbol, string Name, decimal Quantity);
}