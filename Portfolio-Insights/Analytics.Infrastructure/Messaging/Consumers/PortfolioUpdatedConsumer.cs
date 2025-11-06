using MassTransit;
using BuildingBlocks.Messaging.Events;

namespace Analytics.Infrastructure.Messaging.Consumers
{
    public class PortfolioUpdatedConsumer : IConsumer<PortfolioUpdatedEvent>
    {
        public PortfolioUpdatedConsumer() { }

        public async Task Consume(ConsumeContext<PortfolioUpdatedEvent> context)
        {
            var message = context.Message;
            // TODO: Forward to Application layer service when ready
            await Task.CompletedTask;
        }
    }
}