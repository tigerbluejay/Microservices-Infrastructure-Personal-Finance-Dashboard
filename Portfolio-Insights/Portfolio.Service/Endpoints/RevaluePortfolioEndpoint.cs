using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.DTOs;
using Portfolio.Service.Handlers;

namespace Portfolio.Service.Endpoints
{
    public class RevaluePortfolioEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/portfolio/{user}/revalue", async (string user, ISender sender) =>
            {
                var cmd = new RevaluePortfolioCommand(user);
                var result = await sender.Send(cmd);
                return Results.Ok(result);
            })
            .WithName("RevaluePortfolio")
            .Produces<List<PortfolioValueDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Revalue a user's portfolio")
            .WithDescription("Fetches latest prices from MarketData service and returns updated asset values.");
        }
    }
}