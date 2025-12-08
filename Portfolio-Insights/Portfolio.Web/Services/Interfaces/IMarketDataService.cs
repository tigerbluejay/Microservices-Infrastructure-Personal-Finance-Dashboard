using Portfolio.Web.Models.MarketData;
using Refit;

namespace Portfolio.Web.Services.Interfaces
{
    public interface IMarketDataService
    {
        // Temporary placeholder endpoint
        [Get("/health")]
        Task<string> HealthCheckAsync();

        [Post("/marketdata-service/api/marketdata/simulate")]
        Task<MarketSimulationResponseDto> SimulateAsync();
    }
}
