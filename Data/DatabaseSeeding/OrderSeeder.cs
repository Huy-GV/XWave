using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Data.DatabaseSeeding
{
    public static class OrderSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using (var context = new XWaveDbContext(
                serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>()))
            {
                CreatePayments(context);
                CreateCustomers(context);
                CreateOrders(context);
                CreateOrderDetail(context);
                CreatePaymentDetail(context);
                context.Database.CloseConnection();
            }
        }

        private static void CreatePayments(XWaveDbContext dbContext)
        {
            var payments = new List<Payment>()
            {
                new Payment()
                {
                    Provider = "mastercard",
                    AccountNo = 12345678,
                    ExpiryDate = DateTime.Parse("2/5/2023"),
                    
                },
                new Payment()
                {
                    Provider = "visa",
                    AccountNo = 24681357,
                    ExpiryDate = DateTime.Parse("1/2/2023"),
                },
            };

            //dbContext.Database.OpenConnection();
            //dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Payment ON");
            dbContext.Payment.AddRange(payments);
            dbContext.SaveChanges();
            //dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Payment OFF");
        }
        private static void CreateCustomers(XWaveDbContext dbContext)
        {
            var customers = new List<Customer>()
            {
                new Customer()
                {
                    ID = 1,
                    Country = "Australia",
                    PhoneNumber = 12345678,
                    Address = "02 Main St VIC"
                },
                new Customer()
                {
                    ID = 2,
                    Country = "Australia",
                    PhoneNumber = 98765432,
                    Address = "15 Second St VIC"
                },
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Customer ON");
            dbContext.Customer.AddRange(customers);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Customer OFF");
        }
        private static void CreateOrders(XWaveDbContext dbContext)
        {
            var orders = new List<Order>()
            {
                new Order()
                {
                    Date = DateTime.Parse("15/11/2021"),
                    CustomerID = 1,
                    PaymentID = 1
                },
                new Order()
                {
                    Date = DateTime.Parse("21/10/2021"),
                    CustomerID = 1,
                    PaymentID = 1
                },
                new Order()
                {
                    Date = DateTime.Parse("16/9/2021"),
                    CustomerID = 2,
                    PaymentID = 2
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
                    OrderID = 1,
                    ProductID = 1,
                    PriceAtOrder = 200,
                    Quantity = 1,
                },
                new OrderDetail()
                {
                    OrderID = 2,
                    ProductID = 3,
                    PriceAtOrder = 90,
                    Quantity = 2,
                },
                new OrderDetail()
                {
                    OrderID = 3,
                    ProductID = 4,
                    PriceAtOrder = 1600,
                    Quantity = 1,
                },
            };

            dbContext.OrderDetail.AddRange(orderDetail);
            dbContext.SaveChanges();
        }
        private static void CreatePaymentDetail(XWaveDbContext dbContext)
        {
            var paymentDetail = new List<PaymentDetail>()
            {
                new PaymentDetail
                {
                    PaymentID = 1,
                    CustomerID = 1,
                    PurchaseCount = 1,
                    Registration = DateTime.Parse("2/1/2021"),
                    LatestPurchase = DateTime.Now
                },
                new PaymentDetail
                {
                    CustomerID = 2,
                    PaymentID = 2,
                    PurchaseCount = 5,
                    Registration = DateTime.Parse("18/6/2021"),
                    LatestPurchase = DateTime.Now
                }
            };

            dbContext.PaymentDetail.AddRange(paymentDetail);
            dbContext.SaveChanges();
        }

    }
}
