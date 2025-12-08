namespace Portfolio.Web.Models.Analytics;

public class AnalyticsSummaryDto
{
    public string UserName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal DailyChangePercent { get; set; }
    public decimal TotalReturnPercent { get; set; }

    public List<AssetContributionDto> AssetContributions { get; set; } = new();
}

public class AssetContributionDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal WeightPercent { get; set; }
}