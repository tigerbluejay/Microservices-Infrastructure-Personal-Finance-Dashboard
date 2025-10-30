using MarketData.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Data
{
    public class MarketDataContext : DbContext
    {

        public MarketDataContext(DbContextOptions<MarketDataContext> options) : base(options)
        {

        }

        /// <summary>
        /// Table containing simulation logs.
        /// </summary>
        public DbSet<Logs> Logs { get; set; } = default!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Logs>(b =>
            {
                b.ToTable("Logs");
                b.HasKey(l => l.Id);
                b.Property(l => l.Asset).IsRequired().HasMaxLength(32);
                b.Property(l => l.Price).HasColumnType("decimal(18,6)");
                b.Property(l => l.Timestamp).IsRequired();
                b.Property(l => l.Metadata).HasMaxLength(256);
            });
        }
    }
}
