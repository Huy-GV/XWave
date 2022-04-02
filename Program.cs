using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XWave.Data;
using XWave.Data.DatabaseSeeding;

namespace XWave
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<XWaveDbContext>();
                context.Database.EnsureDeleted();
                context.Database.Migrate();

                UserSeeder.SeedData(services);
                ProductRelatedDataSeeder.SeedData(services);
                PurchaseRelatedDataSeeder.SeedData(services);
                StaffActivitySeeder.SeedData(services);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}