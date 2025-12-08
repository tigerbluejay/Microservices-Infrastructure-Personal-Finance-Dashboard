using Refit;

namespace Portfolio.Web.Services.Interfaces
{
    public interface IPortfolioService
    {
        // Temporary placeholder endpoint
        [Get("/health")]
        Task<string> HealthCheckAsync();
    }
}
