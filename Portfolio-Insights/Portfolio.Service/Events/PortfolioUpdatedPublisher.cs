using MassTransit;
using Microsoft.Extensions.Logging;
using Portfolio.Service.DTOs;
using BuildingBlocks.Messaging.DTOs;
using BuildingBlocks.Messaging.Events;

namespace Portfolio.Service.Events
{
    public class PortfolioUpdatedPublisher : IPortfolioUpdatedPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<PortfolioUpdatedPublisher> _logger;

        public PortfolioUpdatedPublisher(IBus bus, ILogger<PortfolioUpdatedPublisher> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task PublishAsync(string userName, List<PortfolioAssetDto> assets)
        {
            var @event = new PortfolioUpdatedEvent(userName, assets);

            _logger.LogInformation(
                "Publishing PortfolioUpdatedEvent for user {UserName} with {Count} assets",
                userName,
                assets.Count);

            await _bus.Publish(@event);

            _logger.LogInformation(
                "PortfolioUpdatedEvent successfully published for user {UserName}",
                userName);
        }
    }
}