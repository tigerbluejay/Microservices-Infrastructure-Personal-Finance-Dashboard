using System;


namespace Analytics.Domain.ValueObjects
{
    /// <summary>
    /// Simple value object for user name. Keeps domain-level validation here.
    /// </summary>
    public sealed class UserName : IEquatable<UserName>
    {
        public string Value { get; }


        public UserName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("UserName cannot be empty", nameof(value));
            if (value.Length > 200) throw new ArgumentException("UserName too long", nameof(value));
            Value = value;
        }


        public override bool Equals(object? obj) => Equals(obj as UserName);
        public bool Equals(UserName? other) => other != null && StringComparer.Ordinal.Equals(Value, other.Value);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;


        public static implicit operator string(UserName userName) => userName.Value;
        public static explicit operator UserName(string value) => new UserName(value);
    }
}