using System;
using Analytics.Domain.Abstractions;
using Analytics.Domain.ValueObjects;


namespace Analytics.Domain.Events
{
    /// <summary>
    /// Domain event emitted when analytics have been computed for a user.
    /// </summary>
    public sealed class AnalyticsComputedDomainEvent : IDomainEvent
    {
        public UserName User { get; }
        public AnalyticsId AnalyticsId { get; }
        public decimal TotalValue { get; }
        public decimal DailyChangePercent { get; }
        public decimal TotalReturnPercent { get; }
        public DateTime OccurredOn { get; }


        public AnalyticsComputedDomainEvent(
        UserName user,
        AnalyticsId analyticsId,
        decimal totalValue,
        decimal dailyChangePercent,
        decimal totalReturnPercent,
        DateTime? occurredOn = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            AnalyticsId = analyticsId;
            TotalValue = totalValue;
            DailyChangePercent = dailyChangePercent;
            TotalReturnPercent = totalReturnPercent;
            OccurredOn = occurredOn ?? DateTime.UtcNow;
        }
    }
}