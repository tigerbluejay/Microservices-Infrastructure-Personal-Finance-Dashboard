using System;
using System.Collections.Generic;

namespace Analytics.Application.DTOs
{
    public record PortfolioAnalyticsSnapshotDTO(
        string UserName,
        DateTime Timestamp,
        decimal TotalValue,
        decimal DailyChangePercent,
        decimal TotalReturnPercent
    );
}