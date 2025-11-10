using Analytics.Application.Data;
using Analytics.Application.DTOs;
using Carter;
using Microsoft.EntityFrameworkCore;

namespace Analytics.API.Endpoints
{
    public record GetAnalyticsByUserResponse(
        string UserName,
        decimal TotalValue,
        decimal DailyChangePercent,
        decimal TotalReturnPercent,
        IEnumerable<AssetContributionDTO> AssetContributions
    );
    public class GetAnalyticsByUser : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/analytics/{userName}", async (string userName, IAnalyticsDbContext dbContext) =>
            {
                var analytics = await dbContext.PortfolioAnalytics
                   .Include(a => a.AssetContributions)
                   .AsNoTracking() // <-- prevent EF from trying to add to fixed-size collections
                   .FirstOrDefaultAsync(a => a.User.Value == userName);

                if (analytics is null)
                    return Results.NotFound($"No analytics found for user '{userName}'");

                var response = new GetAnalyticsByUserResponse(
                    analytics.User.Value,
                    analytics.TotalValue,
                    analytics.DailyChangePercent,
                    analytics.TotalReturnPercent,
                    analytics.AssetContributions
                .Select(ac => new AssetContributionDTO(ac.Symbol, ac.CurrentValue, ac.WeightPercent))
);

                return Results.Ok(response);
            })
            .WithName("GetAnalyticsByUser")
            .Produces<GetAnalyticsByUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Analytics by User")
            .WithDescription("Retrieves current portfolio analytics for a user.");
        }
    }
}