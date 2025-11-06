

// File: ValueObjects/AnalyticsId.cs
using System;


namespace Analytics.Domain.ValueObjects
{
    /// <summary>
    /// Strongly typed identifier for analytics aggregates.
    /// </summary>
    public readonly struct AnalyticsId : IEquatable<AnalyticsId>
    {
        public Guid Value { get; }


        public AnalyticsId(Guid value)
        {
            if (value == Guid.Empty) throw new ArgumentException("AnalyticsId cannot be empty guid", nameof(value));
            Value = value;
        }


        public static AnalyticsId New() => new(Guid.NewGuid());
        public static AnalyticsId FromGuid(Guid guid) => new(guid);


        public override string ToString() => Value.ToString();
        public override int GetHashCode() => Value.GetHashCode();
        public bool Equals(AnalyticsId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is AnalyticsId other && Equals(other);


        public static implicit operator Guid(AnalyticsId id) => id.Value;
        public static explicit operator AnalyticsId(Guid g) => new AnalyticsId(g);
    }
}