using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.Data.DatabaseSeeding.Seeders;
using Microsoft.Extensions.Hosting;

namespace XWave.Core.Extension;

public static class ServiceCollectionExtension
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

    public static void AddDataSeeder(this IServiceCollection services, string environmentName)
    {
        if (environmentName == Environments.Development)
        {
            services.AddTransient<IDataSeeder, DevelopmentDataSeeder>();
        }
        else
        {
            services.AddTransient<IDataSeeder, ProductionDataSeeder>();
        }
    }

    public static void AddHangFireBackgroundServices(this IServiceCollection services, string dbConnectionString)
    {
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