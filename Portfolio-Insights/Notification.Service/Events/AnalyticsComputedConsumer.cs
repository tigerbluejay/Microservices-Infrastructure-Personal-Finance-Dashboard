using MassTransit;
using MediatR;
using BuildingBlocks.Messaging.Events;


namespace NotificationService.Events
{
    public class AnalyticsComputedConsumer : IConsumer<AnalyticsComputedEvent>
    {
        private readonly ISender _sender;


        public AnalyticsComputedConsumer(ISender sender)
        {
            _sender = sender;
        }


        public async Task Consume(ConsumeContext<AnalyticsComputedEvent> context)
        {
            await _sender.Send(new HandleAnalyticsComputedEvent(context.Message));
        }
    }
}