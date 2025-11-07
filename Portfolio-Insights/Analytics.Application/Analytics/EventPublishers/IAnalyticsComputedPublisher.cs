using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;

namespace Analytics.Application.EventPublishers
{
    public interface IAnalyticsComputedPublisher
    {
        Task PublishAsync(AnalyticsComputedEvent eventMessage);
    }
}