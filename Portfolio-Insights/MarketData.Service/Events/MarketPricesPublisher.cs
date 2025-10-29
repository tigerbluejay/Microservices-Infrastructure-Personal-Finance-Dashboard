using MassTransit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Events;

namespace MarketData.Service.Events
{
    /// <summary>
    /// Concrete implementation of IMarketPricesPublisher that uses MassTransit
    /// to publish MarketPricesUpdatedEvent messages to RabbitMQ.
    /// </summary>
    public class MarketPricesPublisher : IMarketPricesPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MarketPricesPublisher> _logger;

        public MarketPricesPublisher(IPublishEndpoint publishEndpoint, ILogger<MarketPricesPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task PublishAsync(List<MarketPriceDto> prices)
        {
            if (prices == null || prices.Count == 0)
            {
                _logger.LogWarning("Attempted to publish MarketPricesUpdatedEvent with no prices.");
                return;
            }

            var @event = new MarketPricesUpdatedEvent(prices);

            _logger.LogInformation(
                "Publishing MarketPricesUpdatedEvent with {Count} assets at {Timestamp}",
                prices.Count,
                @event.OccurredOn
            );

            await _publishEndpoint.Publish(@event);

            _logger.LogInformation("MarketPricesUpdatedEvent successfully published.");
        }
    }
}