using System.Threading.Tasks;
using MassTransit;
using BuildingBlocks.Messaging.Events;
using Analytics.Application.EventPublishers;

namespace Analytics.Infrastructure.Messaging.Publishers
{
    public class AnalyticsComputedPublisher : IAnalyticsComputedPublisher
    {
        private readonly IBus _bus;

        public AnalyticsComputedPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task PublishAsync(AnalyticsComputedEvent eventMessage)
        {
            await _bus.Publish(eventMessage);
        }
    }
}