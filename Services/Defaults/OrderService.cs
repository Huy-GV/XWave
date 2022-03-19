using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Data;
using XWave.Services.Interfaces;
using System.Linq;
using XWave.DTOs.Customers;
using System;
using XWave.ViewModels.Customers;
using Microsoft.Extensions.Logging;
using XWave.Services.ResultTemplate;
using XWave.DTOs.Customers;

namespace XWave.Services.Defaults
{
    public class OrderService : IOrderService
    {
        private readonly XWaveDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        public OrderService(
            XWaveDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<OrderDto?> FindOrderByOrderIdAsync(string customerId, int orderId)
        {
            var orderDTOs = await FindAllOrdersAsync(customerId);
            return orderDTOs.FirstOrDefault(o => o.Id == orderId);
        }
        public async Task<ServiceResult> CreateOrderAsync(
            PurchaseViewModel purchaseViewModel, 
            string customerId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var customer = await _dbContext.CustomerAccount
                    .SingleOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                {
                    return ServiceResult.Failure("Customer not found");
                }
                 
                var payment = await _dbContext.PaymentAccount
                    .SingleOrDefaultAsync(p => p.Id == purchaseViewModel.PaymentAccountId);

                if (payment == null)
                {
                    return ServiceResult.Failure("Payment not found");
                }
                    
                var order = new Order()
                {
                    CustomerId = customerId,
                    PaymentAccountId = purchaseViewModel.PaymentAccountId,
                };

                List<Product> purchasedProducts = new();
                List<OrderDetails> orderDetails = new();

                foreach (var purchasedProduct in purchaseViewModel.Cart)
                {
                    var product = await _dbContext.Product
                        .Include(p => p.Discount)
                        .SingleOrDefaultAsync(p => p.Id == purchasedProduct.ProductId);
                    if (product == null)
                    {
                        return ServiceResult.Failure("Ordered product not found");
                    }

                    if (product.Quantity < purchasedProduct.Quantity)
                    {
                        return ServiceResult.Failure("Quantity exceeded existing stock");
                    }

                    //prevent customers from ordering based on incorrect data
                    if (product.Price != purchasedProduct.DisplayedPrice ||
                        product.Discount?.Percentage != purchasedProduct.DisplayedDiscount)
                    {
                        return ServiceResult.Failure("Conflicting data about product");
                    }

                    product.Quantity -= purchasedProduct.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetails
                    {
                        Quantity = purchasedProduct.Quantity,
                        ProductId = purchasedProduct.ProductId,
                        PriceAtOrder = product.Price - product.Price * product.Discount.Percentage / 100,
                    });
                }

                _dbContext.Order.Add(order);
                //call SaveChanges to get the generated Id
                await _dbContext.SaveChangesAsync();

                orderDetails = AssignOrderId(order.Id, orderDetails);

                _dbContext.OrderDetails.AddRange(orderDetails);
                _dbContext.Product.UpdateRange(purchasedProducts);
                await UpdateTransactionDetailsAsync(purchaseViewModel.PaymentAccountId, customerId);
                await _dbContext.SaveChangesAsync();
                transaction.Commit();

                return ServiceResult.Success(order.Id.ToString());

            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception.Message);

                return ServiceResult.Failure(exception.Message);
            }
        }
        private static List<OrderDetails> AssignOrderId(int orderId, List<OrderDetails> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.OrderId = orderId;
            }
                
            return orderDetails;
        }

        private async Task UpdateTransactionDetailsAsync(
            int paymentId,
            string customerId)
        {
            var transactionDetails = await _dbContext.TransactionDetails.SingleAsync(
                pd => pd.PaymentAccountId == paymentId && pd.CustomerId == customerId);

            transactionDetails.PurchaseCount++;
            transactionDetails.LatestPurchase = DateTime.Now;
            transactionDetails.TransactionType = TransactionType.Purchase;
            _dbContext.TransactionDetails.Update(transactionDetails);
        }

        public async Task<IEnumerable<OrderDetails>> FindAllOrderDetailsAsync()
        {
            return await _dbContext.OrderDetails.ToListAsync();
        }

        public Task<IEnumerable<OrderDto>> FindAllOrdersAsync(string customerId)
        {
            
            var orderDtos =  _dbContext.Order
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerId)
                .Select(o => new OrderDto()
                {
                    Id = o.Id,
                    OrderDate = o.Date,
                    AccountNo = o.Payment.AccountNumber,
                    OrderDetails = o
                        .OrderDetails
                        .Select(od => new OrderDetailDto()
                        {
                            Quantity = od.Quantity,
                            Price = od.PriceAtOrder,
                            ProductName = od.Product.Name
                        })
                })
                .AsEnumerable();

            return Task.FromResult(orderDtos);
        }

        public async Task<OrderDetails> FindOrderDetailsByIdsAsync(int orderId, int productId)
        {
            return await _dbContext.OrderDetails.FirstOrDefaultAsync(
                od => od.ProductId == productId && od.OrderId == orderId);
        }
    }
}
