using BuildingBlocks.CQRS;

namespace MarketData.Service.Handlers
{
    // Command record (input)
    public record SimulateCommand() : ICommand<SimulateResult>;

    // Result record (output)
    public record SimulateResult(bool Success, DateTime Timestamp);
}