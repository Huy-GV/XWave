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
        public async Task<OrderDto> GetOrderByIdAsync(string customerId, int orderId)
        {
            var orderDTOs = await GetAllOrdersAsync(customerId);
            return orderDTOs.FirstOrDefault(o => o.Id == orderId);
        }
        public async Task<ServiceResult> CreateOrderAsync(
            PurchaseViewModel purchaseViewModel, 
            string customerId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            string savepoint = "BeforePurchaseConfirmation";

            transaction.CreateSavepoint(savepoint);
            try
            {
                var customer = await _dbContext.Customer
                    .SingleOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                {
                    return ServiceResult.Failure("Customer not found");
                }
                 
                var payment = await _dbContext.Payment
                    .SingleOrDefaultAsync(p => p.Id == purchaseViewModel.PaymentId);

                if (payment == null)
                {
                    return ServiceResult.Failure("Payment not found");
                }
                    
                var order = new Order()
                {
                    Date = DateTime.Now,
                    CustomerId = customerId,
                    PaymentAccountId = purchaseViewModel.PaymentId,
                };

                List<Product> purchasedProducts = new();
                List<OrderDetail> orderDetails = new();

                foreach (var purchasedProduct in purchaseViewModel.ProductCart)
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
                    orderDetails.Add(new OrderDetail
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

                _dbContext.OrderDetail.AddRange(orderDetails);
                _dbContext.Product.UpdateRange(purchasedProducts);
                await UpdateTransactionDetailsAsync(purchaseViewModel.PaymentId, customerId);
                await _dbContext.SaveChangesAsync();
                transaction.Commit();

                return ServiceResult.Success(order.Id.ToString());

            }
            catch (Exception exception)
            {
                await transaction.RollbackToSavepointAsync(savepoint);
                _logger.LogError(exception.Message);

                return ServiceResult.Failure(exception.Message);
            }
        }
        private static List<OrderDetail> AssignOrderId(int orderId, List<OrderDetail> orderDetails)
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
            var transactionDetails = await _dbContext.PaymentDetail.SingleAsync(
                pd => pd.PaymentAccountId == paymentId && pd.CustomerId == customerId);

            transactionDetails.PurchaseCount++;
            transactionDetails.LatestPurchase = DateTime.Now;
            transactionDetails.TransactionType = TransactionType.Purchase;
            _dbContext.PaymentDetail.Update(transactionDetails);
        }

        public async Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync()
        {
            return await _dbContext.OrderDetail.ToListAsync();
        }

        public Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string customerId)
        {
            
            var orderDtos =  _dbContext.Order
                .AsNoTracking()
                .Include(o => o.OrderDetailCollection)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerId)
                .Select(o => new OrderDto()
                {
                    Id = o.Id,
                    OrderDate = o.Date,
                    AccountNo = o.Payment.AccountNo,
                    OrderDetailCollection = o
                        .OrderDetailCollection
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

        public async Task<OrderDetail> GetDetailsByOrderIdsAsync(int orderId, int productId)
        {
            return await _dbContext.OrderDetail.FirstOrDefaultAsync(
                od => od.ProductId == productId && od.OrderId == orderId);
        }
    }
}
