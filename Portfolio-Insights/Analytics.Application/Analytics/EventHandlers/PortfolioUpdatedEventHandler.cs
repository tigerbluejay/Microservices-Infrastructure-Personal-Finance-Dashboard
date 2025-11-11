using Analytics.Application.Data;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analytics.Application.Analytics.EventHandlers
{
    public interface IPortfolioUpdatedEventHandler
    {
        Task Handle(PortfolioUpdatedEvent @event, CancellationToken cancellationToken = default);
    }

    public class PortfolioUpdatedEventHandler : IPortfolioUpdatedEventHandler
    {
        private readonly IAnalyticsDbContext _dbContext;
        private readonly MarketDataGrpcClient _marketDataClient;

        public PortfolioUpdatedEventHandler(IAnalyticsDbContext dbContext, MarketDataGrpcClient marketDataClient)
        {
            _dbContext = dbContext;
            _marketDataClient = marketDataClient;
        }

        public async Task Handle(PortfolioUpdatedEvent @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"📩 PortfolioUpdatedEvent received for user '{@event.UserName}' with {@event.Assets.Count} assets.");

            // Get or create analytics aggregate
            var analytics = await _dbContext.PortfolioAnalytics
                .FirstOrDefaultAsync(a => a.User.Value == @event.UserName, cancellationToken);

            if (analytics == null)
            {
                analytics = new PortfolioAnalytics(AnalyticsId.New(), new UserName(@event.UserName));
                _dbContext.PortfolioAnalytics.Add(analytics);
            }

            // Fetch latest prices (looping one by one through the gRPC client)
            var symbols = @event.Assets.Select(a => a.Symbol).Distinct().ToList();
            var prices = new Dictionary<string, decimal>();

            foreach (var symbol in symbols)
            {
                try
                {
                    var price = await _marketDataClient.GetPriceAsync(symbol);
                    prices[symbol] = price;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to fetch price for {symbol}: {ex.Message}");
                    prices[symbol] = 0m; // fallback to 0 if price retrieval fails
                }
            }

            // Compute asset values
            var assetValues = @event.Assets.Select(a =>
            {
                var price = prices.TryGetValue(a.Symbol, out var p) ? p : 0m;
                var currentValue = price * a.Quantity;
                return (a.Symbol, currentValue);
            });

            // Recompute analytics
            analytics.ComputeFromCurrentValues(assetValues);

            // Persist
            await _dbContext.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"✅ Portfolio analytics updated for '{@event.UserName}'.");
        }
    }
}