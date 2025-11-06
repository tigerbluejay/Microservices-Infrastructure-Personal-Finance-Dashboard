using System;


namespace Analytics.Domain.Abstractions
{
    /// <summary>
    /// Marker interface for domain events with a timestamp.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}