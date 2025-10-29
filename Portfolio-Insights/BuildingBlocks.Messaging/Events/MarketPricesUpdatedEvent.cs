// we use primitive types (string, int, etc.) for all
// properties to ensure compatibility across different services and platforms.
using System.Collections.Generic;

namespace BuildingBlocks.Messaging.Events
{
    
    /// <summary>
    /// Integration event published by the Market Data Service every time
    /// asset prices are updated (either automatically or manually triggered).
    /// Consumed by the Analytics Service and others interested in market data updates.
    /// </summary>
    public record MarketPricesUpdatedEvent(List<MarketPriceDto> Prices) : IntegrationEvent;

    /// <summary>
    /// Represents a lightweight DTO containing asset symbol and price.
    /// Used for messaging between services (RabbitMQ via MassTransit).
    /// </summary>
    public record MarketPriceDto(string Symbol, decimal Price);
}