using System.Runtime.CompilerServices;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using XWave.Core.Data;
using XWave.Core.Data.DatabaseSeeding;
using XWave.Core.Models;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.Data.DatabaseSeeding.Seeders;

namespace XWave.Core.Extension;

public static class XWaveServiceExtension
{
    public static void AddDefaultXWaveServices(this IServiceCollection services)
    {
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentAccountService, PaymentAccountService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IStaffAccountService, StaffAccountService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddTransient<ProductDtoMapper>();
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<XWaveDbContext>();

        services.AddDbContext<XWaveDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
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
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(dbConnectionString);
        });
        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
        });
    }
}