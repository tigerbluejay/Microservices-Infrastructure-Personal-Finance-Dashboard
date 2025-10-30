using Carter;
using Mapster;
using MediatR;
using MarketData.Service.Handlers;
using Microsoft.AspNetCore.Http;

namespace MarketData.Service.Endpoints
{
    public record SimulateRequest(); // empty request, since this endpoint doesn’t take input

    public record SimulateResponse(bool Success, DateTime Timestamp);

    public class SimulateEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/marketdata/simulate", async (SimulateRequest request, ISender sender) =>
            {
                var command = request.Adapt<SimulateCommand>(); // just like UpdateProductCommand
                var result = await sender.Send(command);
                var response = result.Adapt<SimulateResponse>();

                return Results.Ok(response);
            })
            .WithName("SimulateMarketUpdate")
            .Produces<SimulateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Manually trigger a market data simulation")
            .WithDescription("Randomly updates market prices and publishes MarketPricesUpdatedEvent to RabbitMQ.");
        }
    }
}