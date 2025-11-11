using MassTransit;
using BuildingBlocks.Messaging.Events;
using Analytics.Application.Analytics.EventHandlers; // For the handler
using MediatR; // Needed if you use IMediator inside consumer
using System.Threading.Tasks;

namespace Analytics.Infrastructure.Messaging.Consumers
{
    public class PortfolioUpdatedConsumer : IConsumer<PortfolioUpdatedEvent>
    {
        private readonly IPortfolioUpdatedEventHandler _handler;

        public PortfolioUpdatedConsumer(IPortfolioUpdatedEventHandler handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<PortfolioUpdatedEvent> context)
        {
            var message = context.Message;
            await _handler.Handle(message, context.CancellationToken);
        }
    }
}