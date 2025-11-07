using Analytics.Application.Data;
using Analytics.Application.DTOs;
using Analytics.Application.Exceptions;
using Analytics.Application.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analytics.Application.Analytics.Queries
{
    public class GetAnalyticsByUserQueryHandler : IRequestHandler<GetAnalyticsByUserQuery, PortfolioAnalyticsDTO>
    {
        private readonly IAnalyticsDbContext _dbContext;

        public GetAnalyticsByUserQueryHandler(IAnalyticsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PortfolioAnalyticsDTO> Handle(GetAnalyticsByUserQuery request, CancellationToken cancellationToken)
        {
            var analytics = await _dbContext.PortfolioAnalytics
                .Where(a => a.User == request.UserName)
                .OrderByDescending(a => a.LastUpdatedUtc) // Assuming your aggregate has CreatedAt
                .FirstOrDefaultAsync(cancellationToken);

            if (analytics == null)
                throw new AnalyticsNotFoundException($"No analytics found for user '{request.UserName}'.");

            return analytics.ToDTO();
        }
    }
}