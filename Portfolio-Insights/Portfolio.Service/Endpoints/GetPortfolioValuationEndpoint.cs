using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.DTOs;
using Portfolio.Service.Handlers;

namespace Portfolio.Service.Endpoints
{
    public class GetPortfolioValuationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/portfolio/{user}/valuation", async (string user, ISender sender) =>
            {
                var query = new GetPortfolioValuationQuery(user);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetPortfolioValuation")
            .Produces<List<PortfolioValueDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a user's portfolio valuation")
            .WithDescription("Fetches latest prices from MarketData service and returns calculated asset values.");
        }
    }
}