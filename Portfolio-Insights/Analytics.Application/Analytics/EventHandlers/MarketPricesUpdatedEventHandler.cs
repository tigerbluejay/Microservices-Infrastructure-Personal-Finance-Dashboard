using Analytics.Application.Analytics.Commands;
using BuildingBlocks.Messaging.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Analytics.Application.Analytics.EventHandlers
{
    /// <summary>
    /// Handles market price updates coming from the Market Data Service.
    /// Recomputes analytics for affected users.
    /// </summary>
    public class MarketPricesUpdatedEventHandler
    {
        private readonly IMediator _mediator;

        public MarketPricesUpdatedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(MarketPricesUpdatedEvent @event, CancellationToken cancellationToken)
        {
            // Convert price list to a dictionary for convenient lookup
            var priceDict = @event.Prices.ToDictionary(p => p.Symbol, p => p.Price);

            // TODO: later fetch affected users from DB or cache.
            var affectedUsers = await GetAffectedUsersAsync(@event.Prices);

            // Temporary placeholder assets — replace later with real user portfolios
            var emptyAssets = new List<BuildingBlocks.Messaging.DTOs.PortfolioAssetDto>();

            foreach (var userName in affectedUsers)
            {
                var command = new ComputeAnalyticsCommand(userName, emptyAssets, priceDict);
                await _mediator.Send(command, cancellationToken);
            }
        }

        /// <summary>
        /// Placeholder: returns all users for now.
        /// Later, filter only users who own assets in the updated price list.
        /// </summary>
        private Task<List<string>> GetAffectedUsersAsync(List<MarketPriceDto> prices)
        {
            // TODO: integrate with Portfolio or Analytics DB
            return Task.FromResult(new List<string> { "demo_user" });
        }
    }
}