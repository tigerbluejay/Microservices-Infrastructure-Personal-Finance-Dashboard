namespace Portfolio.Service.DTOs
{
    public record PortfolioValueDto(
        string Symbol,
        string Name,
        decimal Quantity,
        decimal Price,
        decimal Value
    );
}