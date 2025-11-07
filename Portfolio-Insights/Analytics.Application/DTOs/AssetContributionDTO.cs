namespace Analytics.Application.DTOs
{
    public record AssetContributionDTO(
        string Symbol,
        decimal Value,
        decimal WeightPercent
    );
}