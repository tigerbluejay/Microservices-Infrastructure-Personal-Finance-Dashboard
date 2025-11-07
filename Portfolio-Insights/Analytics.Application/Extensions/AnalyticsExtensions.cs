using System.Linq;
using Analytics.Application.DTOs;
using Analytics.Domain.Models;

namespace Analytics.Application.Extensions
{
    public static class AnalyticsExtensions
    {
        public static PortfolioAnalyticsDTO ToDTO(this PortfolioAnalytics analytics)
        {
            return new PortfolioAnalyticsDTO(
                analytics.Id.Value,                             // Guid
                analytics.User.Value,                           // string
                analytics.TotalValue,
                analytics.AssetContributions.Select(a => new AssetContributionDTO(
                    a.Symbol,
                    a.CurrentValue,
                    a.WeightPercent
                )).ToList(),
                analytics.LastUpdatedUtc                        // DateTime
            );
        }

        public static PortfolioAnalyticsSnapshotDTO ToSnapshotDTO(this PortfolioAnalytics analytics)
        {
            return new PortfolioAnalyticsSnapshotDTO(
                analytics.User.Value,
                analytics.LastUpdatedUtc,
                analytics.TotalValue,
                analytics.DailyChangePercent,
                analytics.TotalReturnPercent
            );
        }
    }
}