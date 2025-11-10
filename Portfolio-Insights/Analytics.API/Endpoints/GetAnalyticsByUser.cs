using Analytics.Application.Data;
using Microsoft.EntityFrameworkCore;
using Carter;

namespace Analytics.API.Endpoints
{
    public record GetAnalyticsByUserResponse(string UserName, decimal TotalValue, decimal DailyChangePercent, decimal TotalReturnPercent);

    public class GetAnalyticsByUser : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/analytics/{userName}", async (string userName, IAnalyticsDbContext dbContext) =>
            {
                var analytics = await dbContext.PortfolioAnalytics
                    .Include(a => a.AssetContributions)
                    .FirstOrDefaultAsync(a => a.User.Value == userName);

                if (analytics is null)
                    return Results.NotFound($"No analytics found for user '{userName}'");

                var response = new GetAnalyticsByUserResponse(
                    analytics.User.Value,
                    analytics.TotalValue,
                    analytics.DailyChangePercent,
                    analytics.TotalReturnPercent
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