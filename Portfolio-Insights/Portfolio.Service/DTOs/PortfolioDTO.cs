using System.Collections.Generic;

namespace Portfolio.Service.DTOs
{
    public record PortfolioDto(string UserName, List<PortfolioValueDto> Assets, decimal TotalValue);
}