using Analytics.Application.Analytics.Commands;
using Analytics.Application.Data;
using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Analytics.Application.Analytics.EventHandlers
{
    /// <summary>
    /// Handles MarketPricesUpdatedEvent messages from the Market Data Service.
    /// Recomputes analytics for all affected users.
    /// </summary>
    public interface IMarketPricesUpdatedHandler
    {
        Task Handle(MarketPricesUpdatedEvent @event, CancellationToken cancellationToken = default);
    }

    public class MarketPricesUpdatedHandler : IMarketPricesUpdatedHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MarketPricesUpdatedHandler> _logger;
        private readonly IAnalyticsDbContext _dbContext;

        public MarketPricesUpdatedHandler(
            IMediator mediator,
            ILogger<MarketPricesUpdatedHandler> logger,
            IAnalyticsDbContext dbContext)
        {
            _mediator = mediator;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(MarketPricesUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[Analytics] Handling MarketPricesUpdatedEvent with {Count} prices at {Timestamp}",
                @event.Prices.Count, System.DateTime.UtcNow);

            var priceDict = @event.Prices.ToDictionary(p => p.Symbol, p => p.Price);

            var portfolios = await _dbContext.PortfolioAnalytics
                .Include(p => p.AssetContributions)
                .ToListAsync(cancellationToken);

            foreach (var portfolio in portfolios)
            {
                if (!portfolio.AssetContributions.Any())
                {
                    _logger.LogInformation("[Analytics] No assets for user {UserName}, skipping", portfolio.User.Value);
                    continue;
                }

                var assets = portfolio.AssetContributions
                    .Select(a => new BuildingBlocks.Messaging.DTOs.PortfolioAssetDto
                    {
                        Symbol = a.Symbol,
                        Quantity = 1 // TODO: replace with real quantity if available
                    })
                    .ToList();

                var command = new ComputeAnalyticsCommand(
                    portfolio.User.Value,
                    assets,
                    priceDict
                );

                await _mediator.Send(command, cancellationToken);

                _logger.LogInformation("[Analytics] Finished computing analytics for user {UserName}", portfolio.User.Value);
            }
        }
    }
}