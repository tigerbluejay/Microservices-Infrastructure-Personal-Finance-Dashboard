using System;
using System.Collections.Generic;

namespace Analytics.Application.DTOs
{
    public record PortfolioAnalyticsDTO(
        Guid Id,
        string UserName,
        decimal TotalValue,
        List<AssetContributionDTO> AssetContributions,
        DateTime LastUpdatedUtc
    );
}