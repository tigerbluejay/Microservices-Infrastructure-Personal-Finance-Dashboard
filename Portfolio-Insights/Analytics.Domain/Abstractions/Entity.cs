using System;

namespace Analytics.Domain.Abstractions
{
    /// <summary>
    /// Basic entity base class. Uses a typed id to improve clarity in domain models.
    /// </summary>
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }


        protected Entity() { }


        protected Entity(TId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            Id = id;
        }
    }
}