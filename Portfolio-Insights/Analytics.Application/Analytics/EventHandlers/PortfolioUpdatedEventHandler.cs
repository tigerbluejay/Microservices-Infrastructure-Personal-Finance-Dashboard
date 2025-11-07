using Analytics.Application.Analytics.Commands;
using BuildingBlocks.Messaging.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Analytics.Application.Analytics.EventHandlers
{
    /// <summary>
    /// Handles events from the Portfolio Service when a user's portfolio has changed.
    /// Triggers computation of updated analytics for that user.
    /// </summary>
    public class PortfolioUpdatedEventHandler
    {
        private readonly IMediator _mediator;

        public PortfolioUpdatedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(PortfolioUpdatedEvent @event, CancellationToken cancellationToken)
        {
            // For now, prices are not passed — the Application layer can later fetch them via gRPC.
            var command = new ComputeAnalyticsCommand(
                @event.UserName,
                @event.Assets
            );

            await _mediator.Send(command, cancellationToken);
        }
    }
}