using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Data.DatabaseSeeding
{
    public static class PurchaseRelatedDataSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using (var context = new XWaveDbContext(
                serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>()))
            {
                var userManager = serviceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

                try
                {
                    CreatePayments(context);
                    CreateOrders(context, userManager).Wait();
                    CreateOrderDetail(context);
                    CreatePaymentDetail(context, userManager).Wait();
                } catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding purchase data");
                    logger.LogError(ex.Message);
                } finally
                {
                    context.Database.CloseConnection();
                }

            }
        }

        private static void CreatePayments(XWaveDbContext dbContext)
        {
            var payments = new List<PaymentAccount>()
            {
                new PaymentAccount()
                {
                    Provider = "mastercard",
                    AccountNo = "12345678",
                    ExpiryDate = DateTime.Parse("2/5/2023"),

                },
                new PaymentAccount()
                {
                    Provider = "visa",
                    AccountNo = "24681357",
                    ExpiryDate = DateTime.Parse("1/2/2023"),
                },
            };

            dbContext.Payment.AddRange(payments);
            dbContext.SaveChanges();
        }
        private static async Task CreateOrders(
            XWaveDbContext dbContext, 
            UserManager<ApplicationUser> userManager)
        {
            var user1 = await userManager.FindByNameAsync("john_customer");
            var user2 = await userManager.FindByNameAsync("jake_customer");

            var orders = new List<Order>()
            {
                new Order()
                {
                    Date = DateTime.Parse("15/11/2021"),
                    CustomerId = user1.Id,
                    PaymentAccountId = 1
                },
                new Order()
                {
                    Date = DateTime.Parse("21/10/2021"),
                    CustomerId = user1.Id,
                    PaymentAccountId = 1
                },
                new Order()
                {
                    Date = DateTime.Parse("16/9/2021"),
                    CustomerId = user2.Id,
                    PaymentAccountId = 2
                }
            };
            dbContext.Order.AddRange(orders);
            dbContext.SaveChanges();
        }
        private static void CreateOrderDetail(XWaveDbContext dbContext)
        {
            var orderDetail = new List<OrderDetail>()
            {
                new OrderDetail()
                {
                    OrderId = 1,
                    ProductId = 1,
                    PriceAtOrder = 200,
                    Quantity = 1,
                },
                new OrderDetail()
                {
                    OrderId = 1,
                    ProductId = 3,
                    PriceAtOrder = 90,
                    Quantity = 2,
                },
                new OrderDetail()
                {
                    OrderId = 2,
                    ProductId = 4,
                    PriceAtOrder = 1600,
                    Quantity = 1,
                },
                new OrderDetail()
                {
                    OrderId = 3,
                    ProductId = 4,
                    PriceAtOrder = 1600,
                    Quantity = 4,
                },
                new OrderDetail()
                {
                    OrderId = 3,
                    ProductId = 2,
                    PriceAtOrder = 40,
                    Quantity = 9,
                },
                new OrderDetail()
                {
                    OrderId = 3,
                    ProductId = 3,
                    PriceAtOrder = 600,
                    Quantity = 15,
                },
            };

            dbContext.OrderDetail.AddRange(orderDetail);
            dbContext.SaveChanges();
        }
        private static async Task CreatePaymentDetail(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            var user1 = await userManager.FindByNameAsync("john_customer");
            var user2 = await userManager.FindByNameAsync("jake_customer");
            var paymentDetail = new List<TransactionDetails>()
            {
                new TransactionDetails
                {
                    PaymentAccountId = 1,
                    CustomerId =  user1.Id,
                    PurchaseCount = 1,
                    Registration = DateTime.Parse("2/1/2021"),
                    LatestPurchase = DateTime.Now,
                    TransactionType = TransactionType.Purchase
                },
                new TransactionDetails
                {
                    CustomerId = user2.Id,
                    PaymentAccountId = 2,
                    PurchaseCount = 5,
                    Registration = DateTime.Parse("18/6/2021"),
                    LatestPurchase = DateTime.Now,
                    TransactionType = TransactionType.Purchase
                }
            };

            dbContext.PaymentDetail.AddRange(paymentDetail);
            dbContext.SaveChanges();
        }

    }
}
