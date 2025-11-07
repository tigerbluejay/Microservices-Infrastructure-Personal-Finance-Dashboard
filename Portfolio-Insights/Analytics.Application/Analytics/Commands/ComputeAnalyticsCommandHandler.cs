using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analytics.Application.Data;
using Analytics.Application.Services;
using Analytics.Application.EventPublishers;
using BuildingBlocks.Messaging.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Analytics.Commands
{
    /// <summary>
    /// Handles computation of analytics whenever a portfolio or market event occurs.
    /// </summary>
    public class ComputeAnalyticsCommandHandler : IRequestHandler<ComputeAnalyticsCommand, Unit>
    {
        private readonly IAnalyticsDbContext _dbContext;
        private readonly IAnalyticsCalculator _calculator;
        private readonly IAnalyticsComputedPublisher _publisher;

        public ComputeAnalyticsCommandHandler(
            IAnalyticsDbContext dbContext,
            IAnalyticsCalculator calculator,
            IAnalyticsComputedPublisher publisher)
        {
            _dbContext = dbContext;
            _calculator = calculator;
            _publisher = publisher;
        }

        public async Task<Unit> Handle(ComputeAnalyticsCommand request, CancellationToken cancellationToken)
        {
            // Fetch current analytics for the user (if any)
            var existingAnalytics = await _dbContext.PortfolioAnalytics
                .FirstOrDefaultAsync(a => a.User.Value == request.UserName, cancellationToken);

            // Compute updated analytics using the calculator
            var computedAnalytics = await _calculator.ComputeAsync(
                request.UserName,
                request.Assets,
                request.LatestPrices
            );

            if (existingAnalytics is null)
            {
                // First-time creation
                _dbContext.PortfolioAnalytics.Add(computedAnalytics);
            }
            else
            {
                // Update existing analytics
                existingAnalytics.Reset();
                existingAnalytics.ComputeFromCurrentValues(
                    computedAnalytics.AssetContributions.Select(a => (a.Symbol, a.CurrentValue))
                );
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Publish integration event for other services (e.g., Notifications)
            var analyticsComputedEvent = new AnalyticsComputedEvent(
                computedAnalytics.User.Value,
                computedAnalytics.TotalValue,
                computedAnalytics.DailyChangePercent,
                computedAnalytics.TotalReturnPercent,
                computedAnalytics.LastUpdatedUtc
            );

            await _publisher.PublishAsync(analyticsComputedEvent);

            return Unit.Value;
        }
    }
}