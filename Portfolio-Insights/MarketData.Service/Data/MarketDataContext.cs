using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Data
{
    public class MarketDataContext : DbContext
    {

        public MarketDataContext(DbContextOptions<MarketDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
