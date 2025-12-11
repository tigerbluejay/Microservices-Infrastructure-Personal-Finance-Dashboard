using Portfolio.Web.Models.Analytics;
using Refit;

namespace Portfolio.Web.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // Temporary placeholder endpoint
        [Get("/health")]
        Task<string> HealthCheckAsync();

        [Get("/analytics-service/api/analytics/{userName}")]
        Task<AnalyticsSummaryDto> GetSummaryAsync(string userName);

        [Get("/analytics-service/api/analytics/{userName}/history")]
        Task<List<PortfolioHistorySnapshotDto>> GetHistoryAsync(string userName);

        [Post("/analytics-service/api/analytics/refresh")]
        Task<string> RefreshAnalyticsAsync([Body] RefreshAnalyticsRequestDto request);
    }
}
