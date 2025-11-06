using Analytics.Domain.Abstractions;
using Analytics.Domain.Models;
using Analytics.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Analytics.Infrastructure.Data
{
    public class AnalyticsDbContext : DbContext
    {
        private readonly AuditableEntityInterceptor _auditInterceptor;
        private readonly DispatchDomainEventInterceptor _dispatchInterceptor;

        public DbSet<PortfolioAnalytics> PortfolioAnalytics => Set<PortfolioAnalytics>();

        public AnalyticsDbContext(
            DbContextOptions<AnalyticsDbContext> options,
            AuditableEntityInterceptor auditInterceptor,
            DispatchDomainEventInterceptor dispatchInterceptor)
            : base(options)
        {
            _auditInterceptor = auditInterceptor;
            _dispatchInterceptor = dispatchInterceptor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor, _dispatchInterceptor);
        }
    }
}