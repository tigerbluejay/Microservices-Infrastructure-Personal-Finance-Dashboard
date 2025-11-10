using Analytics.Application.Data;
using Microsoft.EntityFrameworkCore;
using Carter;

namespace Analytics.API.Endpoints
{
    public record AnalyticsSnapshotResponse(DateTime Timestamp, decimal TotalValue);

    public class GetAnalyticsHistory : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/analytics/{userName}/history", async (string userName, IAnalyticsDbContext dbContext) =>
            {
                var analytics = await dbContext.PortfolioAnalytics
                    .Include(a => a.Snapshots)
                    .FirstOrDefaultAsync(a => a.User.Value == userName);

                if (analytics is null)
                    return Results.NotFound($"No analytics history found for user '{userName}'");

                var response = analytics.Snapshots
                    .OrderByDescending(s => s.Timestamp)
                    .Select(s => new AnalyticsSnapshotResponse(s.Timestamp, s.TotalValue));

                return Results.Ok(response);
            })
            .WithName("GetAnalyticsHistory")
            .Produces<IEnumerable<AnalyticsSnapshotResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Analytics History")
            .WithDescription("Retrieves historical portfolio analytics snapshots for a user.");
        }
    }
}