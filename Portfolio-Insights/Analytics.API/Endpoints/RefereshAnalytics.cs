using Analytics.Application.Analytics.Commands;
using MediatR;
using Carter;

namespace Analytics.API.Endpoints
{
    public record RefreshAnalyticsRequest(string UserName, List<BuildingBlocks.Messaging.DTOs.PortfolioAssetDto> Assets);

    public class RefreshAnalytics : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/analytics/refresh", async (RefreshAnalyticsRequest request, ISender sender) =>
            {
                var command = new ComputeAnalyticsCommand(
                    request.UserName,
                    request.Assets,
                    latestPrices: null
                );

                await sender.Send(command);

                return Results.Ok("Analytics recomputed successfully.");
            })
            .WithName("RefreshAnalytics")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Refresh Analytics")
            .WithDescription("Forces recalculation of analytics for a given user.");
        }
    }
}