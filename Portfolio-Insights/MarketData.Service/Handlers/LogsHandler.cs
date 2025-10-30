using BuildingBlocks.CQRS;
using MarketData.Service.Data;
using MarketData.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Handlers
{
    /// <summary>
    /// Query to get the latest N logs
    /// </summary>
    public record GetLogsQuery(int Limit = 10) : IQuery<List<Logs>>;

    public class LogsHandler : IQueryHandler<GetLogsQuery, List<Logs>>
    {
        private readonly MarketDataContext _context;

        public LogsHandler(MarketDataContext context)
        {
            _context = context;
        }

        public async Task<List<Logs>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            var limit = request.Limit <= 0 ? 10 : request.Limit;

            return await _context.Logs
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .ThenByDescending(l => l.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
    }
}