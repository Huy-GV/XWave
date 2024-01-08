using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class PurchaseRelatedDataSeeder
{
    public static async Task SeedData<TSeeder>(
        XWaveDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TSeeder> logger) where TSeeder : IDataSeeder
    {
        try
        {
            var user1 = await userManager.FindByNameAsync("john_customer");
            var user2 = await userManager.FindByNameAsync("jake_customer");

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
            var paymentAccounts = await CreatePaymentAccountsAsync(context);
            var orders = await CreateOrdersAsync(context, paymentAccounts, users);
            await CreateOrderDetailAsync(context, orders, products);
            await CreatePaymentDetailAsync(context, users, paymentAccounts);
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while seeding purchase data");
            throw;
        }
    }

    private static async Task<List<PaymentAccount>> CreatePaymentAccountsAsync(XWaveDbContext dbContext)
    {
        var paymentAccounts = TestPaymentAccountFactory.PaymentAccounts();
        dbContext.PaymentAccount.AddRange(paymentAccounts);
        await dbContext.SaveChangesAsync();
        return paymentAccounts;
    }

    private static async Task<List<Order>> CreateOrdersAsync(
        XWaveDbContext dbContext,
        List<PaymentAccount> paymentAccounts,
        List<ApplicationUser> users)
    {
        var orders = TestOrderFactory.Orders(users, paymentAccounts);
        dbContext.Order.AddRange(orders);
        await dbContext.SaveChangesAsync();
        return orders;
    }

    private static async Task CreateOrderDetailAsync(XWaveDbContext dbContext, List<Order> orders, List<Product> products)
    {
        dbContext.OrderDetails.AddRange(TestOrderFactory.OrderDetails(products, orders));
        await dbContext.SaveChangesAsync();
    }

    private static async Task CreatePaymentDetailAsync(
        XWaveDbContext dbContext,
        List<ApplicationUser> users,
        List<PaymentAccount> paymentAccounts)
    {
        var paymentAccountDetails = TestPaymentAccountFactory.PaymentAccountDetails(paymentAccounts, users);
        dbContext.PaymentAccountDetails.AddRange(paymentAccountDetails);
        await dbContext.SaveChangesAsync();
    }
}