using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DailyChangePercent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    TotalReturnPercent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WeightPercent = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PortfolioAnalyticsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetContributions_PortfolioAnalytics_PortfolioAnalyticsId",
                        column: x => x.PortfolioAnalyticsId,
                        principalTable: "PortfolioAnalytics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioAnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PortfolioAnalyticsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAnalyticsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioAnalyticsSnapshots_PortfolioAnalytics_PortfolioAnalyticsId",
                        column: x => x.PortfolioAnalyticsId,
                        principalTable: "PortfolioAnalytics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetContributions_PortfolioAnalyticsId",
                table: "AssetContributions",
                column: "PortfolioAnalyticsId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAnalyticsSnapshots_PortfolioAnalyticsId",
                table: "PortfolioAnalyticsSnapshots",
                column: "PortfolioAnalyticsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetContributions");

            migrationBuilder.DropTable(
                name: "PortfolioAnalyticsSnapshots");

            migrationBuilder.DropTable(
                name: "PortfolioAnalytics");
        }
    }
}
