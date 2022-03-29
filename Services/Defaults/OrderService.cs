using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Customers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customers;

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

        public async Task<OrderDto?> FindOrderByIdAsync(string customerId, int orderId)
        {
            var orderDTOs = await FindAllOrdersAsync(customerId);
            return orderDTOs.FirstOrDefault(o => o.Id == orderId);
        }

        public async Task<(ServiceResult, int? OrderId)> AddOrderAsync(
            PurchaseViewModel purchaseViewModel,
            string customerId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var customer = await _dbContext.CustomerAccount.FindAsync(customerId);
                if (customer == null)
                {
                    return (ServiceResult.Failure("Customer not found"), null);
                }

                var payment = await _dbContext.PaymentAccount
                    .SingleOrDefaultAsync(p => p.Id == purchaseViewModel.PaymentAccountId);

                if (payment == null)
                {
                    return (ServiceResult.Failure("Payment not found"), null);
                }

                var order = new Order()
                {
                    CustomerId = customerId,
                    PaymentAccountId = purchaseViewModel.PaymentAccountId,
                };

                var purchasedProducts = new List<Product>();
                var orderDetails = new List<OrderDetails>();

                foreach (var purchasedProduct in purchaseViewModel.Cart)
                {
                    var product = await _dbContext.Product
                        .Include(p => p.Discount)
                        .SingleOrDefaultAsync(p => p.Id == purchasedProduct.ProductId);

                    if (product == null)
                    {
                        return (ServiceResult.Failure("Ordered product not found."), null);
                    }

                    if (product.Quantity < purchasedProduct.Quantity)
                    {
                        return (ServiceResult.Failure("Quantity exceeded existing stock."), null);
                    }

                    //prevent customers from ordering based on incorrect data
                    if (product.Discount?.Percentage != purchasedProduct.DisplayedDiscountPercentage)
                    {
                        return (ServiceResult.Failure($"Discount percentage has been changed during transaction. Please view the latest price for the item {product.Name}."), null);
                    }

                    if (product.Price != purchasedProduct.DisplayedPrice)
                    {
                        return (ServiceResult.Failure($"Price has been changed during transaction. Please view the latest price for the item {product.Name}."), null);
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
                await _dbContext.SaveChangesAsync();
                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.OrderId = order.Id;
                }

                _dbContext.OrderDetails.AddRange(orderDetails);
                _dbContext.Product.UpdateRange(purchasedProducts);
                var succeeded = await UpdateTransactionDetailsAsync(purchaseViewModel.PaymentAccountId, customerId);
                if (!succeeded)
                {
                    return (ServiceResult.Failure("Failed to update transaction. Operation is aborted."), null);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (ServiceResult.Success(), order.Id);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception.Message);

                return (ServiceResult.Failure("An error occured when placing your order."), null);
            }
        }

        /// <summary>
        /// Records the latest transaction made by a customer.
        /// </summary>
        /// <param name="paymentAccountId">ID of payment account used in the latest transaction.</param>
        /// <param name="customerId">ID of customer who made the transaction.</param>
        /// <returns>True if the update succeeds and False otherwise.</returns>
        private async Task<bool> UpdateTransactionDetailsAsync(int paymentAccountId, string customerId)
        {
            var transactionDetails = await _dbContext.TransactionDetails.FindAsync(customerId, paymentAccountId);
            if (transactionDetails == null)
            {
                return false;
            }

            transactionDetails.PurchaseCount++;
            transactionDetails.LatestPurchase = DateTime.Now;
            transactionDetails.TransactionType = TransactionType.Purchase;
            _dbContext.TransactionDetails.Update(transactionDetails);

            return true;
        }

        public Task<IEnumerable<OrderDto>> FindAllOrdersAsync(string customerId)
        {
            var orderDtos = _dbContext.Order
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .IgnoreQueryFilters()
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

        public async Task<OrderDetails?> FindPurchasedProductDetailsByOrderId(int orderId, int productId)
        {
            return await _dbContext.OrderDetails.FindAsync(orderId, productId);
        }
    }
}