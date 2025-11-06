using System;


namespace Analytics.Domain.Models
{
    /// <summary>
    /// Snapshot of the portfolio total value at a point in time.
    /// Used for history/line chart.
    /// </summary>
    public class PortfolioAnalyticsSnapshot
    {
        public DateTime Timestamp { get; init; }
        public decimal TotalValue { get; init; }


        public PortfolioAnalyticsSnapshot(DateTime timestamp, decimal totalValue)
        {
            Timestamp = timestamp;
            TotalValue = totalValue;
        }
    }
}