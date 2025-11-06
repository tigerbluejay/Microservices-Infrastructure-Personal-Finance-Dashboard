using System;


namespace Analytics.Domain.Models
{
    /// <summary>
    /// Represents an asset's contribution to portfolio value.
    /// </summary>
    public class AssetContribution
    {
        public string Symbol { get; init; } = default!;
        public decimal WeightPercent { get; init; }
        public decimal CurrentValue { get; init; }


        public AssetContribution(string symbol, decimal currentValue, decimal weightPercent)
        {
            Symbol = string.IsNullOrWhiteSpace(symbol) ? throw new ArgumentException("Symbol required", nameof(symbol)) : symbol;
            CurrentValue = currentValue;
            WeightPercent = weightPercent;
        }
    }
}