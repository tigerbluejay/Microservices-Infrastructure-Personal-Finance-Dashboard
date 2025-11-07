namespace BuildingBlocks.Messaging.Events
{
    public record AnalyticsComputedEvent(
        string UserName,
        decimal TotalValue,
        decimal DailyChangePercent,
        decimal TotalReturnPercent,
        DateTime ComputedAtUtc
    );
}