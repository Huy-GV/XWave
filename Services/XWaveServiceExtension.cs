using Microsoft.Extensions.DependencyInjection;
using XWave.Services.Defaults;
using XWave.Services.Interfaces;

namespace XWave.Services
{
    public static class XWaveServiceExtension
    {
        public static void AddDefaultServices(this IServiceCollection services)
        {
            //TODO: move all service registration here

            services.AddScoped<IStaffActivityService, StaffActivityService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IDiscountService, DiscountService>();
        }
    }
}
