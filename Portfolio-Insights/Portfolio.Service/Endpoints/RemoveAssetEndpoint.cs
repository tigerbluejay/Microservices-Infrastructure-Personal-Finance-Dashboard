using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.Handlers;

namespace Portfolio.Service.Endpoints
{
    public class RemoveAssetEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/portfolio/{user}/asset/{symbol}", async (string user, string symbol, ISender sender) =>
            {
                var cmd = new RemoveAssetCommand(user, symbol);
                await sender.Send(cmd);
                return Results.NoContent();
            })
            .WithName("RemoveAsset")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Remove an asset from a portfolio by symbol")
            .WithDescription("Removes the specified asset by its symbol and publishes a PortfolioUpdatedEvent.");
        }
    }
}