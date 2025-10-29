using BuildingBlocks.Messaging.Events;

namespace MarketData.Service.Events
{
    /// <summary>
    /// Abstraction for publishing market price update events
    /// to the message broker (RabbitMQ via MassTransit).
    /// Keeps the rest of the service decoupled from messaging implementation details.
    /// </summary>
    public interface IMarketPricesPublisher
    {
        /// <summary>
        /// Publishes an event containing the latest updated market prices.
        /// </summary>
        /// <param name="prices">Collection of asset prices to include in the event.</param>
        Task PublishAsync(List<MarketPriceDto> prices);
    }
}