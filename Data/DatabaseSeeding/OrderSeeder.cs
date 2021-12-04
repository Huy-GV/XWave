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
            using (var context = new ApplicationDbContext(
                serviceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                CreatePayments(context);
                CreateCustomers(context);
                CreateOrders(context);  
                CreateOrderDetail(context);
                context.Database.CloseConnection();
            }
        }

        private static void CreatePayments(ApplicationDbContext dbContext)
        {
            var payments = new List<Payment>()
            {
                new Payment()
                {
                    ID = 1,
                    Provider = "mastercard",
                    ExpiryDate = DateTime.Parse("2/5/2023"),
                    AccountNumber = 12345678
                },
                new Payment()
                {
                    ID = 2,
                    Provider = "visa",
                    ExpiryDate = DateTime.Parse("1/2/2023"),
                    AccountNumber = 24681357
                },
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Payment ON");
            dbContext.Payment.AddRange(payments);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Payment OFF");
        }
        private static void CreateCustomers(ApplicationDbContext dbContext)
        {
            var customers = new List<Customer>()
            {
                new Customer()
                {
                    ID = 1,
                    Country = "Australia",
                    PaymentID = 1
                },
                new Customer()
                {
                    ID = 2,
                    Country = "Australia",
                    PaymentID = 2
                },
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Customer ON");
            dbContext.Customer.AddRange(customers);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Customer OFF");
        }
        private static void CreateOrders(ApplicationDbContext dbContext)
        {
            var orders = new List<Order>()
            {
                new Order()
                {
                    // ID = 1,
                    Date = DateTime.Parse("15/11/2021"),
                    CustomerID = 1,
                    PaymentID = 1
                },
                new Order()
                {
                    // ID = 2,
                    Date = DateTime.Parse("21/10/2021"),
                    CustomerID = 1,
                    PaymentID = 1
                },
                new Order()
                {
                    // ID = 3,
                    Date = DateTime.Parse("16/9/2021"),
                    CustomerID = 2,
                    PaymentID = 2
                }
            };
            // dbContext.Database.OpenConnection();
            // dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Order ON");
            dbContext.Order.AddRange(orders);
            dbContext.SaveChanges();
            // dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Order OFF");
        }
        private static void CreateOrderDetail(ApplicationDbContext dbContext)
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
            // dbContext.Database.OpenConnection();
            // dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.OrderDetail ON");
            dbContext.OrderDetail.AddRange(orderDetail);
            dbContext.SaveChanges();
            // dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.OrderDetail OFF");
        }


    }
}
