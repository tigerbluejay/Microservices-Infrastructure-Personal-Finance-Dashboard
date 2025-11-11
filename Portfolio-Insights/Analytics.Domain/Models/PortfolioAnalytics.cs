using System;
using System.Collections.Generic;
using System.Linq;
using Analytics.Domain.Abstractions;
using Analytics.Domain.Events;
using Analytics.Domain.ValueObjects;
using Analytics.Domain.Exceptions;


namespace Analytics.Domain.Models
{
    /// <summary>
    /// Aggregate root representing computed analytics for a user portfolio.
    /// </summary>
    public class PortfolioAnalytics : AggregateRoot<AnalyticsId>
    {
        private List<AssetContribution> _assetContributions = new();

        public UserName User { get; private set; } = default!;

        public decimal TotalValue { get; private set; }
        public decimal DailyChangePercent { get; private set; }
        public decimal TotalReturnPercent { get; private set; }
                public IReadOnlyList<AssetContribution> AssetContributions
        {
            get => _assetContributions;
            private set => _assetContributions = value.ToList(); // EF materialization works
        }
        public DateTime LastUpdatedUtc { get; private set; }

        // optional: keep snapshot history in-memory for domain logic; persistence layer should store it.
        private readonly List<PortfolioAnalyticsSnapshot> _snapshots = new();
        public IReadOnlyCollection<PortfolioAnalyticsSnapshot> Snapshots => _snapshots.AsReadOnly();


        public PortfolioAnalytics(AnalyticsId id, UserName user) : base(id)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            LastUpdatedUtc = DateTime.MinValue;
        }


        /// <summary>
        /// Recompute analytics from asset current values.
        /// </summary>
        /// <param name="assetValues">Pair of symbol->value for current total value per asset (e.g., quantity*price)</param>
        /// <param name="previousTotalValue">Previous day's total value, used to compute daily change. Null if unknown.</param>
        /// <param name="initialTotalValue">Initial portfolio value for total return calculation. Null if unknown.</param>
        public void ComputeFromCurrentValues(IEnumerable<(string Symbol, decimal CurrentValue)> assetValues, decimal? previousTotalValue = null, decimal? initialTotalValue = null)
        {
            if (assetValues == null) throw new ArgumentNullException(nameof(assetValues));

            var items = assetValues.ToList();

            var total = items.Sum(x => x.CurrentValue);

            if (total < 0) throw new DomainException("Total portfolio value cannot be negative.");

            TotalValue = total;

            DailyChangePercent = previousTotalValue.HasValue && previousTotalValue.Value > 0
            ? Decimal.Round((TotalValue - previousTotalValue.Value) / previousTotalValue.Value * 100m, 4)
            : 0m;

            TotalReturnPercent = initialTotalValue.HasValue && initialTotalValue.Value > 0
            ? Decimal.Round((TotalValue - initialTotalValue.Value) / initialTotalValue.Value * 100m, 4)
            : 0m;

            // compute contributions
            var contributions = new List<AssetContribution>();
            foreach (var it in items)
            {
                var weight = total > 0 ? Decimal.Round(it.CurrentValue / total * 100m, 6) : 0m;
                contributions.Add(new AssetContribution(it.Symbol, it.CurrentValue, weight));
            }

            AssetContributions = contributions;

            // snapshot
            var snapshot = new PortfolioAnalyticsSnapshot(DateTime.UtcNow, TotalValue);
            _snapshots.Add(snapshot);
            LastUpdatedUtc = snapshot.Timestamp;

            // publish domain event
            var evt = new AnalyticsComputedDomainEvent(User, Id, TotalValue, DailyChangePercent, TotalReturnPercent, LastUpdatedUtc);
            AddDomainEvent(evt);
        }

        // EF Core needs this
        private PortfolioAnalytics() { }

        /// <summary>
        /// Clears any computed analytics and domain events.
        /// </summary>
        public void Reset()
        {
            TotalValue = 0m;
            DailyChangePercent = 0m;
            TotalReturnPercent = 0m;
            AssetContributions = Array.Empty<AssetContribution>();
            _snapshots.Clear();
            ClearDomainEvents();
        }
    }
}