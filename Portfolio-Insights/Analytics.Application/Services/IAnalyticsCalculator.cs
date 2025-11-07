using Analytics.Domain.Models;
using BuildingBlocks.Messaging.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Analytics.Application.Services
{
    /// <summary>
    /// Application service responsible for calculating analytics for a user's portfolio.
    /// </summary>
    public interface IAnalyticsCalculator
    {
        Task<PortfolioAnalytics> ComputeAsync(string userName, List<PortfolioAssetDto> assets, Dictionary<string, decimal>? latestPrices = null);
    }
}