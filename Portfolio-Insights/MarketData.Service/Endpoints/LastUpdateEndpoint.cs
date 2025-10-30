using Carter;
using MediatR;
using MarketData.Service.Handlers;
using Microsoft.AspNetCore.Http;

namespace MarketData.Service.Endpoints
{
    public record LastUpdateResponse(DateTime? Timestamp);

    public class LastUpdateEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/marketdata/lastupdate", async (ISender sender) =>
            {
                var lastUpdate = await sender.Send(new LastUpdateQuery());
                var response = new LastUpdateResponse(lastUpdate);
                return Results.Ok(response);
            })
            .WithName("GetLastMarketUpdate")
            .Produces<LastUpdateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get timestamp of last simulated market update")
            .WithDescription("Returns the UTC timestamp of the last simulation run.");
        }
    }
}