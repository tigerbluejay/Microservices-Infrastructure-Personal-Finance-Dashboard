using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.Handlers;
using Portfolio.Service.DTOs;

namespace Portfolio.Service.Endpoints
{
    public class GetPortfolioEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/portfolio/{user}", async (string user, ISender sender) =>
            {
                var query = new GetPortfolioQuery(user);
                var portfolio = await sender.Send(query);

                if (portfolio == null)
                    return Results.NotFound();

                return Results.Ok(portfolio);
            })
            .WithName("GetPortfolio")
            .Produces<PortfolioDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a user's portfolio")
            .WithDescription("Returns a user's portfolio enriched with live price and value data.");
        }
    }
}