using Analytics.Application.Analytics.Commands;
using Analytics.Infrastructure.Services;
using MediatR;
using Carter;
using Microsoft.AspNetCore.Http;

namespace Analytics.API.Endpoints
{
    public record RefreshAnalyticsRequest(
        string UserName,
        List<BuildingBlocks.Messaging.DTOs.PortfolioAssetDto> Assets
    );

    public class RefreshAnalytics : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/analytics/refresh", async (
                RefreshAnalyticsRequest request,
                ISender sender,
                MarketDataGrpcClient marketDataClient) =>
            {
                // Fetch live prices from MarketData gRPC
                var symbols = request.Assets.Select(a => a.Symbol).ToList();
                var pricesDict = new Dictionary<string, decimal>();
                foreach (var symbol in symbols)
                {
                    pricesDict[symbol] = await marketDataClient.GetPriceAsync(symbol);
                }

                // Create ComputeAnalyticsCommand with quantities and live prices
                var command = new ComputeAnalyticsCommand(
                    request.UserName,
                    request.Assets,
                    latestPrices: pricesDict
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