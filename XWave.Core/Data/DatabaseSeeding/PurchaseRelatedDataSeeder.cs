using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding;

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
            CreatePayments(context);
            CreateOrders(context, userManager).Wait();
            CreateOrderDetail(context);
            CreatePaymentDetail(context, userManager).Wait();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PurchaseRelatedDataSeeder>>();
            logger.LogError(ex, "An error occurred while seeding purchase data");
            logger.LogError(ex.Message);
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }

    private static void CreatePayments(XWaveDbContext dbContext)
    {
        var payments = new List<PaymentAccount>
        {
            new()
            {
                Provider = "mastercard",
                AccountNumber = "12345678",
                ExpiryDate = DateTime.Parse("2/5/2023")
            },
            new()
            {
                Provider = "visa",
                AccountNumber = "24681357",
                ExpiryDate = DateTime.Parse("1/2/2023")
            },
            new()
            {
                Provider = "visa",
                AccountNumber = "01010101",
                ExpiryDate = DateTime.Parse("1/8/2023")
            }
        };

        dbContext.PaymentAccount.AddRange(payments);
        dbContext.SaveChanges();
    }

    private static async Task CreateOrders(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var user1 = await userManager.FindByNameAsync("john_customer");
        var user2 = await userManager.FindByNameAsync("jake_customer");

        var orders = new List<Order>
        {
            new()
            {
                Date = DateTime.Parse("15/11/2021"),
                CustomerId = user1.Id,
                PaymentAccountId = 1,
                DeliveryAddress = "2 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.Parse("21/10/2021"),
                CustomerId = user1.Id,
                PaymentAccountId = 1,
                DeliveryAddress = "3 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.Parse("16/9/2021"),
                CustomerId = user2.Id,
                PaymentAccountId = 2,
                DeliveryAddress = "4 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.Parse("21/11/2021"),
                CustomerId = user1.Id,
                PaymentAccountId = 3,
                DeliveryAddress = "3 Collins St, Melbourne"
            }
        };

        dbContext.Order.AddRange(orders);
        dbContext.SaveChanges();
    }

    private static void CreateOrderDetail(XWaveDbContext dbContext)
    {
        var orderDetail = new List<OrderDetails>
        {
            new()
            {
                OrderId = 1,
                ProductId = 1,
                PriceAtOrder = 200,
                Quantity = 1
            },
            new()
            {
                OrderId = 1,
                ProductId = 3,
                PriceAtOrder = 90,
                Quantity = 2
            },
            new()
            {
                OrderId = 2,
                ProductId = 4,
                PriceAtOrder = 1600,
                Quantity = 1
            },
            new()
            {
                OrderId = 3,
                ProductId = 4,
                PriceAtOrder = 1600,
                Quantity = 4
            },
            new()
            {
                OrderId = 3,
                ProductId = 2,
                PriceAtOrder = 40,
                Quantity = 9
            },
            new()
            {
                OrderId = 3,
                ProductId = 3,
                PriceAtOrder = 600,
                Quantity = 15
            },
            new()
            {
                OrderId = 4,
                ProductId = 3,
                PriceAtOrder = 100,
                Quantity = 2
            },
            new()
            {
                OrderId = 4,
                ProductId = 2,
                PriceAtOrder = 200,
                Quantity = 3
            }
        };

        dbContext.OrderDetails.AddRange(orderDetail);
        dbContext.SaveChanges();
    }

    private static async Task CreatePaymentDetail(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var user1 = await userManager.FindByNameAsync("john_customer");
        var user2 = await userManager.FindByNameAsync("jake_customer");
        var paymentDetail = new List<PaymentAccountDetails>
        {
            new()
            {
                PaymentAccountId = 1,
                CustomerId = user1.Id,
                FirstRegistration = DateTime.Parse("5/1/2020")
            },
            new()
            {
                PaymentAccountId = 3,
                CustomerId = user1.Id,
                FirstRegistration = DateTime.Parse("5/1/2020")
            },
            new()
            {
                CustomerId = user2.Id,
                PaymentAccountId = 2,
                FirstRegistration = DateTime.Parse("5/7/2019")
            }
        };

        dbContext.PaymentAccountDetails.AddRange(paymentDetail);
        dbContext.SaveChanges();
    }
}