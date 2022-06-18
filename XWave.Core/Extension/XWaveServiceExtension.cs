using System.Runtime.CompilerServices;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Data.DatabaseSeeding;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;

namespace XWave.Core.Extension;

public static class XWaveServiceExtension
{
    public static void AddDefaultXWaveServices(this IServiceCollection services)
    {
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IStaffAccountService, StaffAccountService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
    }

    public static void AddDefaultHelpers(this IServiceCollection services)
    {
        services.AddTransient<ProductDtoMapper>();
    }

    public static void SeedDatabase(this IServiceProvider services)
    {
        var context = services.GetRequiredService<XWaveDbContext>();
        context.Database.EnsureDeleted();
        context.Database.Migrate();

        UserSeeder.SeedData(services);
        ProductRelatedDataSeeder.SeedData(services);
        PurchaseRelatedDataSeeder.SeedData(services);
        StaffActivitySeeder.SeedData(services);
    }

    public static void AddHangFireBackgroundServices(this IServiceCollection services, string dbConnectionString)
    {
        services.AddTransient<IBackgroundJobService, HangFireBackgroundJobService>();
        services.AddHangfire(config => { config.UseSqlServerStorage(dbConnectionString); });

        services.AddHangfireServer(options => { options.SchedulePollingInterval = TimeSpan.FromMinutes(1); });
    }
}