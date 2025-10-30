using Carter;
using Mapster;
using MarketData.Service.Handlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Service.Endpoints
{
    public record SimulateResponse(bool Success, DateTime Timestamp);

    public class SimulateEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/marketdata/simulate", async ([FromServices] ISender sender) =>
            {
                // Send the command directly; no body required
                var result = await sender.Send(new SimulateCommand());

                var response = new SimulateResponse(result.Success, result.Timestamp);
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