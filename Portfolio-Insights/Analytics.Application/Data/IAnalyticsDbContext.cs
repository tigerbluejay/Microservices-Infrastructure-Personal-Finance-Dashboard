using Analytics.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Analytics.Application.Data
{
    public interface IAnalyticsDbContext
    {
        DbSet<PortfolioAnalytics> PortfolioAnalytics { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}