using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Data.DatabaseSeeding;
using XWave.Core.Models;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.Data.DatabaseSeeding.Seeders;
using System;

namespace XWave.Core.Extension;

public static class XWaveServiceExtension
{
    public static void AddDefaultXWaveServices(this IServiceCollection services)
    {
        services.AddScoped<IStaffActivityLogger, StaffActivityLogger>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentAccountService, PaymentAccountService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<ICustomerProductBrowser, CustomerProductBrowser>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IStaffAccountService, StaffAccountService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<IAuthenticator, JwtAuthenticator>();
        services.AddScoped<IRoleAuthorizer, RoleAuthorizer>();
        services.AddTransient<IDiscountedProductPriceCalculator, DiscountedProductPriceCalculator>();
        services.AddTransient<ProductDtoMapper>();
    }

    public static void AddDatabase(this IServiceCollection services, string dbConnectionString)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<XWaveDbContext>();

        services.AddDbContext<XWaveDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });
    }

    public static async Task SeedDevelopmentDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<XWaveDbContext>();
        await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();

        UserSeeder.SeedData(services);
        ProductRelatedDataSeeder.SeedData(services);
        PurchaseRelatedDataSeeder.SeedData(services);
        StaffActivitySeeder.SeedData(services);
    }

    public static async Task SeedProductionDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<XWaveDbContext>();
        await context.Database.MigrateAsync();

        UserSeeder.SeedData(services);
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