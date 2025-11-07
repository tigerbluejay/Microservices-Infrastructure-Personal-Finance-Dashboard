using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Analytics.Queries
{
    public record GetAnalyticsByUserQuery(string UserName) : IRequest<PortfolioAnalyticsDTO>;
}