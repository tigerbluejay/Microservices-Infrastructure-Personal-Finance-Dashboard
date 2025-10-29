using MarketData.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Service.Extensions
{
    public static class Extensions
    {
        public static IApplicationBuilder UseMigration(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MarketDataContext>();
                context.Database.MigrateAsync();
            }
            return app;
        }
    }
}
