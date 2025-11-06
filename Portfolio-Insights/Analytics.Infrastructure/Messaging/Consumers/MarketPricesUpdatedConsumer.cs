using MassTransit;
using BuildingBlocks.Messaging.Events;

namespace Analytics.Infrastructure.Messaging.Consumers
{
    public class MarketPricesUpdatedConsumer : IConsumer<MarketPricesUpdatedEvent>
    {
        public MarketPricesUpdatedConsumer() { }

        public async Task Consume(ConsumeContext<MarketPricesUpdatedEvent> context)
        {
            var message = context.Message;
            // TODO: Forward to Application layer service when ready
            await Task.CompletedTask;
        }
    }
}