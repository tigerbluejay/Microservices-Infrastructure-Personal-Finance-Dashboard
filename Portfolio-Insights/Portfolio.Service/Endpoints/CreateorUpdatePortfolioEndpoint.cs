using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Portfolio.Service.DTOs;
using Portfolio.Service.Handlers;

namespace Portfolio.Service.Endpoints
{
    public class CreateOrUpdatePortfolioEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/portfolio/{user}", async (string user, PortfolioDto dto, ISender sender) =>
            {
                var cmd = new CreateOrUpdatePortfolioCommand(user, dto);
                var result = await sender.Send(cmd);
                return Results.Ok(result);
            })
            .WithName("CreateOrUpdatePortfolio")
            .Accepts<PortfolioDto>("application/json")
            .Produces<CreateOrUpdatePortfolioResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Create or update a user's portfolio")
            .WithDescription("Creates a new portfolio for the user or replaces the existing assets.");
        }
    }
}