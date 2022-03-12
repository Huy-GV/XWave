using Microsoft.Extensions.DependencyInjection;
using XWave.Helpers;
using XWave.Services.Defaults;
using XWave.Services.Interfaces;

namespace XWave.Services
{
    public static class XWaveServiceExtension
    {
        public static void AddDefaultXWaveServices(this IServiceCollection services)
        {
            //TODO: move all service registration here

            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IDiscountService, DiscountService>();
        }
        public static void AddDefaultHelpers(this IServiceCollection services)
        {
            services.AddTransient<AuthenticationHelper>();
            services.AddTransient<ProductHelper>();
        }
    }
}
