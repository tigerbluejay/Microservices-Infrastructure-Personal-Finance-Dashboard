using Analytics.Application.Data;
using Analytics.Application.EventPublishers;
using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using BuildingBlocks.Messaging.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
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
            if (request.Assets == null)
                throw new ArgumentNullException(nameof(request.Assets), "Assets cannot be null in ComputeAnalyticsCommand.");

            if (request.LatestPrices == null)
                throw new ArgumentNullException(nameof(request.LatestPrices), "LatestPrices cannot be null in ComputeAnalyticsCommand.");

            var existingAnalytics = await _dbContext.PortfolioAnalytics
                .Include(a => a.AssetContributions)
                .Include(a => a.Snapshots)
                .FirstOrDefaultAsync(a => a.User.Value == request.UserName, cancellationToken);

            var assetValues = request.Assets
                .Where(a => a != null)
                .Select(a =>
                {
                    var price = request.LatestPrices.TryGetValue(a.Symbol, out var p)
                        ? p
                        : 0m;

                    return (Symbol: a.Symbol, CurrentValue: a.Quantity * price);
                })
                .ToList();

            PortfolioAnalytics analytics;
            decimal? previousTotal = null;
            decimal? initialTotal = null;

            if (existingAnalytics is null)
            {
                analytics = new PortfolioAnalytics(
                    new AnalyticsId(),
                    new UserName(request.UserName)
                );

                _dbContext.PortfolioAnalytics.Add(analytics);
            }
            else
            {
                analytics = existingAnalytics;

                previousTotal = analytics.Snapshots.LastOrDefault()?.TotalValue;
                initialTotal = analytics.Snapshots.FirstOrDefault()?.TotalValue;
            }

            analytics.ComputeFromCurrentValues(
                assetValues,
                previousTotal,
                initialTotal
            );

            _dbContext.PortfolioAnalytics.Update(analytics);
            await _dbContext.SaveChangesAsync(cancellationToken);

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