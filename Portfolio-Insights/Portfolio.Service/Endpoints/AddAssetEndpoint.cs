using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.DTOs;
using Portfolio.Service.Handlers;

namespace Portfolio.Service.Endpoints
{
    public class AddAssetEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/portfolio/{user}/asset", async (string user, CreatedAssetDto dto, ISender sender) =>
            {
                var cmd = new AddAssetCommand(user, dto);
                var created = await sender.Send(cmd);
                return Results.Created($"/api/portfolio/{user}/asset/{created.Symbol}", created);
            })
            .WithName("AddAsset")
            .Accepts<CreatedAssetDto>("application/json")
            .Produces<PortfolioValueDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Add an asset to a portfolio")
            .WithDescription("Adds a new asset to the user's portfolio and publishes a PortfolioUpdatedEvent.");
        }
    }
}