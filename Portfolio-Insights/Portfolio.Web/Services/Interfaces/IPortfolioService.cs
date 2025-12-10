using Portfolio.Web.Models.Portfolio;
using Refit;

namespace Portfolio.Web.Services.Interfaces
{
    public interface IPortfolioService
    {
        // Temporary placeholder endpoint
        [Get("/health")]
        Task<string> HealthCheckAsync();

        [Get("/portfolio-service/api/portfolio/{userName}")]
        Task<PortfolioResponseDto> GetPortfolioAsync(string userName);

        [Post("/portfolio-service/api/portfolio/{userName}/asset")]
        Task<AssetDto> AddAssetAsync(string userName, [Body] AddAssetRequestDto request);

        [Delete("/portfolio-service/api/portfolio/{userName}/asset/{symbol}")]
        Task DeleteAssetAsync(string userName, string symbol);

        [Get("/portfolio-service/api/portfolio/{userName}/valuation")]
        Task<List<AssetDto>> RevalueAsync(string userName);
    }
}
