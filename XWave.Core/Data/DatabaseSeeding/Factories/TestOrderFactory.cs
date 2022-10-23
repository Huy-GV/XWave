using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;

public static class TestOrderFactory
{
    public static List<Order> Orders(List<ApplicationUser> users, List<PaymentAccount> paymentAccounts)
    {
        var paymentAccountIds = paymentAccounts.Select(x => x.Id).ToList();
        var userIds = users.Select(x => x.Id).ToList();
        var randomIndex = new Random();
        var orders = new List<Order>
        {
            new()
            {
                Date = DateTime.ParseExact("15/11/2021", "d/M/yyyy", null),
                CustomerId = userIds[randomIndex.Next(userIds.Count)],
                PaymentAccountId = paymentAccountIds[randomIndex.Next(paymentAccountIds.Count)],
                DeliveryAddress = "2 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.ParseExact("21/10/2021", "d/M/yyyy", null),
                CustomerId = userIds[randomIndex.Next(userIds.Count)],
                PaymentAccountId = paymentAccountIds[randomIndex.Next(paymentAccountIds.Count)],
                DeliveryAddress = "3 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.ParseExact("16/9/2021", "d/M/yyyy", null),
                CustomerId = userIds[randomIndex.Next(userIds.Count)],
                PaymentAccountId = paymentAccountIds[randomIndex.Next(paymentAccountIds.Count)],
                DeliveryAddress = "4 Collins St, Melbourne"
            },
            new()
            {
                Date = DateTime.ParseExact("21/11/2021", "d/M/yyyy", null),
                CustomerId = userIds[randomIndex.Next(userIds.Count)],
                PaymentAccountId = paymentAccountIds[randomIndex.Next(paymentAccountIds.Count)],
                DeliveryAddress = "3 Collins St, Melbourne"
            }
        };

        return orders;
    }

    public static List<OrderDetails> OrderDetails(
        List<Product> products,
        List<Order> orders)
    {
        var productIds = products.Select(x => x.Id).ToList();
        var orderIds = orders.Select(x => x.Id).ToList();
        var randomIndex = new Random();
        var orderDetail = new List<OrderDetails>
        {
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 200,
                Quantity = 1
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 90,
                Quantity = 2
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 1600,
                Quantity = 1
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 1600,
                Quantity = 4
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 40,
                Quantity = 9
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 600,
                Quantity = 15
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)] ,
                PriceAtOrder = 100,
                Quantity = 2
            },
            new()
            {
                OrderId = orderIds[randomIndex.Next(orderIds.Count)],
                ProductId = productIds[randomIndex.Next(productIds.Count)],
                PriceAtOrder = 200,
                Quantity = 3
            }
        };

        return orderDetail.DistinctBy(x => new { x.OrderId, x.ProductId }).ToList(); ;
    }
}