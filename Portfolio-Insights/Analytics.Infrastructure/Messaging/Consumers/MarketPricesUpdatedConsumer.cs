using Analytics.Application.Analytics.EventHandlers;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using System.Threading.Tasks;

namespace Analytics.Infrastructure.Messaging.Consumers
{
    public class MarketPricesUpdatedConsumer : IConsumer<MarketPricesUpdatedEvent>
    {
        private readonly IMarketPricesUpdatedHandler _handler;

        public MarketPricesUpdatedConsumer(IMarketPricesUpdatedHandler handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<MarketPricesUpdatedEvent> context)
        {
            await _handler.Handle(context.Message, context.CancellationToken);
        }
    }
}