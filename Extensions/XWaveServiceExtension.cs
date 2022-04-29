using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using XWave.Utils;
using XWave.Services.Defaults;
using XWave.Services.Interfaces;

namespace XWave.Extensions
{
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
        }

        public static void AddDefaultHelpers(this IServiceCollection services)
        {
            services.AddTransient<AuthenticationHelper>();
            services.AddTransient<ProductDtoMapper>();
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
}