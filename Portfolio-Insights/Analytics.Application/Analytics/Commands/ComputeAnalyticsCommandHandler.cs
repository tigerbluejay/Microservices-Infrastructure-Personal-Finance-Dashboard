using Analytics.Application.Data;
using Analytics.Application.EventPublishers;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using BuildingBlocks.Messaging.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analytics.Application.Analytics.Commands
{
    public class ComputeAnalyticsCommandHandler : IRequestHandler<ComputeAnalyticsCommand, Unit>
    {
        private readonly IAnalyticsDbContext _dbContext;
        private readonly IAnalyticsComputedPublisher _publisher;

        public ComputeAnalyticsCommandHandler(
            IAnalyticsDbContext dbContext,
            IAnalyticsComputedPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task<Unit> Handle(ComputeAnalyticsCommand request, CancellationToken cancellationToken)
        {
            // Fetch current analytics for the user
            var existingAnalytics = await _dbContext.PortfolioAnalytics
                .FirstOrDefaultAsync(a => a.User.Value == request.UserName, cancellationToken);

            // Compute asset values using quantities and latest prices
            var assetValues = request.Assets
                .Select(a =>
                {
                    var price = request.LatestPrices[a.Symbol];
                    return (Symbol: a.Symbol, CurrentValue: a.Quantity * price);
                })
                .ToList();

            PortfolioAnalytics analytics;
            if (existingAnalytics is null)
            {
                analytics = new PortfolioAnalytics(
                    new AnalyticsId(),
                    new Domain.ValueObjects.UserName(request.UserName)
                );
            }
            else
            {
                analytics = existingAnalytics;
                analytics.Reset();
            }

            // Compute analytics
            analytics.ComputeFromCurrentValues(assetValues);

            if (existingAnalytics is null)
                _dbContext.PortfolioAnalytics.Add(analytics);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Publish integration event
            await _publisher.PublishAsync(new AnalyticsComputedEvent(
                analytics.User.Value,
                analytics.TotalValue,
                analytics.DailyChangePercent,
                analytics.TotalReturnPercent,
                analytics.LastUpdatedUtc
            ));

            return Unit.Value;
        }
    }
}