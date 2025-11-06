using Analytics.Domain.Models;
using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Data.Configurations
{
    public class PortfolioAnalyticsConfiguration : IEntityTypeConfiguration<PortfolioAnalytics>
    {
        public void Configure(EntityTypeBuilder<PortfolioAnalytics> builder)
        {
            builder.ToTable("PortfolioAnalytics");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedNever();

            builder.OwnsOne(x => x.User, u =>
            {
                u.Property(p => p.Value)
                 .HasColumnName("UserName")
                 .HasMaxLength(200)
                 .IsRequired();
            });

            builder.Property(x => x.TotalValue)
                   .HasColumnType("decimal(18,4)");

            builder.Property(x => x.DailyChangePercent)
                   .HasColumnType("decimal(9,4)");

            builder.Property(x => x.TotalReturnPercent)
                   .HasColumnType("decimal(9,4)");

            builder.Property(x => x.LastUpdatedUtc)
                   .IsRequired();

            //  AssetContributions configuration
            builder.OwnsMany(x => x.AssetContributions, ac =>
            {
                ac.ToTable("AssetContributions");
                ac.WithOwner().HasForeignKey("PortfolioAnalyticsId");

                ac.Property<int>("Id"); // shadow key
                ac.HasKey("Id");

                ac.Property(a => a.Symbol)
                  .HasMaxLength(50)
                  .IsRequired();

                ac.Property(a => a.CurrentValue)
                  .HasColumnType("decimal(18,4)");

                ac.Property(a => a.WeightPercent)
                  .HasColumnType("decimal(9,6)");
            });

            //  Snapshots configuration
            builder.OwnsMany(x => x.Snapshots, s =>
            {
                s.ToTable("PortfolioAnalyticsSnapshots");
                s.WithOwner().HasForeignKey("PortfolioAnalyticsId");

                s.Property<int>("Id");
                s.HasKey("Id");

                s.Property(a => a.Timestamp).IsRequired();
                s.Property(a => a.TotalValue).HasColumnType("decimal(18,4)");
            });

            builder.Property(x => x.Id)
                .HasConversion(
                id => id.Value,
                value => new AnalyticsId(value)
       );
        }
    }
}