using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Analytics.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
    {
        public AnalyticsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();

            // Use your actual connection string here
            optionsBuilder.UseSqlServer("Server=localhost;Database=AnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True;");

            // You can comment these out if interceptors need DI
            return new AnalyticsDbContext(optionsBuilder.Options, null!, null!);
        }
    }
}