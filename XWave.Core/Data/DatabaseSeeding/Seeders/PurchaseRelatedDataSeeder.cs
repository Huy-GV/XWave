using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class PurchaseRelatedDataSeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        try
        {
            var user1 = userManager.FindByNameAsync("john_customer").Result;
            var user2 = userManager.FindByNameAsync("jake_customer").Result;

            if (user1 is null || user2 is null)
            {
                throw new InvalidOperationException("Users are not seeded yet");
            }

            var products = context.Product.ToList();

            if (products.Count == 0)
            {
                throw new InvalidOperationException("Products are not seeded yet");
            }

            var users = new List<ApplicationUser> { user1, user2 };
            var paymentAccounts = CreatePaymentAccounts(context);
            var orders = CreateOrders(context, paymentAccounts, users);
            CreateOrderDetail(context, orders, products);
            CreatePaymentDetail(context, users, paymentAccounts);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PurchaseRelatedDataSeeder>>();
            logger.LogError(ex, "An error occurred while seeding purchase data");
        }
    }

    private static List<PaymentAccount> CreatePaymentAccounts(XWaveDbContext dbContext)
    {
        var paymentAccounts = TestPaymentAccountFactory.PaymentAccounts();
        dbContext.PaymentAccount.AddRange(paymentAccounts);
        dbContext.SaveChanges();
        return paymentAccounts;
    }

    private static List<Order> CreateOrders(
        XWaveDbContext dbContext,
        List<PaymentAccount> paymentAccounts,
        List<ApplicationUser> users)
    {
        var orders = TestOrderFactory.Orders(users, paymentAccounts);
        dbContext.Order.AddRange(orders);
        dbContext.SaveChanges();
        return orders;
    }

    private static void CreateOrderDetail(XWaveDbContext dbContext, List<Order> orders, List<Product> products)
    {
        dbContext.OrderDetails.AddRange(TestOrderFactory.OrderDetails(products, orders));
        dbContext.SaveChanges();
    }

    private static void CreatePaymentDetail(
        XWaveDbContext dbContext,
        List<ApplicationUser> users,
        List<PaymentAccount> paymentAccounts)
    {
        var paymentAccountDetails = TestPaymentAccountFactory.PaymentAccountDetails(paymentAccounts, users);
        dbContext.PaymentAccountDetails.AddRange(paymentAccountDetails);
        dbContext.SaveChanges();
    }
}