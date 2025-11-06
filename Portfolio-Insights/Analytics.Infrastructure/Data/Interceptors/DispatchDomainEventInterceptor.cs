using Analytics.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Analytics.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// After saving changes, publishes domain events via MediatR.
    /// </summary>
    public class DispatchDomainEventInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;

        public DispatchDomainEventInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var aggregates = context.ChangeTracker
             .Entries<IAggregateRoot>()
             .Select(e => e.Entity)
             .Where(e => e.DomainEvents.Any())
             .ToArray();

            foreach (var aggregate in aggregates)
            {
                var events = aggregate.DomainEvents.ToArray();
                aggregate.ClearDomainEvents();

                foreach (var domainEvent in events)
                    await _publisher.Publish(domainEvent, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}