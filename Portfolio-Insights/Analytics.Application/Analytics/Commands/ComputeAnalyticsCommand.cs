using System.Collections.Generic;
using BuildingBlocks.Messaging.DTOs;
using MediatR;

namespace Analytics.Application.Analytics.Commands
{
    public class ComputeAnalyticsCommand : IRequest<Unit>
    {
        public string UserName { get; }
        public List<PortfolioAssetDto> Assets { get; }
        public Dictionary<string, decimal>? LatestPrices { get; }

        public ComputeAnalyticsCommand(
            string userName,
            List<PortfolioAssetDto> assets,
            Dictionary<string, decimal>? latestPrices = null)
        {
            UserName = userName;
            Assets = assets;
            LatestPrices = latestPrices;
        }
    }
}