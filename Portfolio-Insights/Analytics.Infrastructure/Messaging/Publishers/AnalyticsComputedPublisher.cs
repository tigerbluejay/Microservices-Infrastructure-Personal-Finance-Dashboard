using MassTransit;
using Analytics.Domain.Events; // define AnalyticsComputedEvent in Domain or a shared DTO project

namespace Analytics.Infrastructure.Messaging.Publishers
{
    public class AnalyticsComputedPublisher
    {
        private readonly IBus _bus;

        public AnalyticsComputedPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task PublishAsync(AnalyticsComputedDomainEvent @event)
        {
            await _bus.Publish(@event);
        }
    }
}