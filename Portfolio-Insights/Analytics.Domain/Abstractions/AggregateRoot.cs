using System;
using System.Collections.Generic;


namespace Analytics.Domain.Abstractions
{
    /// <summary>
    /// Aggregate root base that captures domain events.
    /// </summary>
    public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();


        protected AggregateRoot() { }


        protected AggregateRoot(TId id) : base(id) { }


        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


        protected void AddDomainEvent(IDomainEvent @event)
        => _domainEvents.Add(@event ?? throw new ArgumentNullException(nameof(@event)));


        protected void RemoveDomainEvent(IDomainEvent @event)
        => _domainEvents.Remove(@event);


        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}