using MediatR;
using BuildingBlocks.Messaging.Events;

namespace NotificationService.Events
{
    public record HandleAnalyticsComputedEvent(
        AnalyticsComputedEvent Event
    ) : IRequest<Unit>;
}