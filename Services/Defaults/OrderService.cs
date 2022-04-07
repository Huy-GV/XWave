using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(
            XWaveDbContext dbContext,
            ILogger<OrderService> logger,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
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
                if (!await _dbContext.CustomerAccount.AnyAsync(c => c.CustomerId == customerId) ||
                    await _userManager.FindByIdAsync(customerId) == null)
                {
                    return (ServiceResult.Failure("Customer account not found."), null);
                }

                if (!await _dbContext.PaymentAccount.AnyAsync(p => p.Id == purchaseViewModel.PaymentAccountId))
                {
                    return (ServiceResult.Failure("Payment not found"), null);
                }

                var order = new Order()
                {
                    CustomerId = customerId,
                    PaymentAccountId = purchaseViewModel.PaymentAccountId,
                    DeliveryAddress = purchaseViewModel.DeliveryAddress,
                };

                var productIdsToPurchase = purchaseViewModel.Cart.Select(p => p.ProductId);
                var productsToPurchase = await _dbContext.Product
                    .Include(p => p.Discount)
                    .Where(p => productIdsToPurchase.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var missingProductNames = productIdsToPurchase.Except(productsToPurchase.Select(p => p.Key));
                if (missingProductNames.Any())
                {
                    return (ServiceResult.Failure($"Error: some ordered products were not found."), null);
                }

                var purchasedProducts = new List<Product>();
                var orderDetails = new List<OrderDetails>();
                var errorMessage = string.Empty;
                var errorMessages = new List<string>();

                foreach (var productInCart in purchaseViewModel.Cart)
                {
                    var product = productsToPurchase[productInCart.ProductId];
                    if (product.Quantity < productInCart.Quantity)
                    {
                        errorMessage += $"Quantity of product named {product.Name} exceeded existing stock.\n";
                        errorMessages.Add($"Quantity of product named {product.Name} exceeded existing stock.");
                    }

                    //prevent customers from ordering based on incorrect data
                    if (product.Discount?.Percentage != productInCart.DisplayedDiscountPercentage)
                    {
                        errorMessage += $"Discount percentage has been changed during the transaction. Please view the latest price for the item {product.Name}.";
                        errorMessages.Add($"Discount percentage has been changed during the transaction. Please view the latest price for the item {product.Name}.");
                    }

                    if (product.Price != productInCart.DisplayedPrice)
                    {
                        errorMessage += $"Price has been changed during the transaction. Please view the latest price for the item {product.Name}.\n";
                        errorMessages.Add($"Price has been changed during the transaction. Please view the latest price for the item {product.Name}.");
                    }

                    if (errorMessages.Any())
                    {
                        // move to validate remaining products without processing any of them.
                        continue;
                    }

                    var purchasePrice = product.Discount == null
                        ? product.Price
                        : product.Price - product.Price * product.Discount.Percentage / 100;
                    product.Quantity -= productInCart.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetails
                    {
                        Quantity = productInCart.Quantity,
                        ProductId = productInCart.ProductId,
                        PriceAtOrder = purchasePrice,
                    });
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return (ServiceResult.Failure(errorMessages.ToArray()), null);
                }

                _dbContext.Order.Add(order);
                await _dbContext.SaveChangesAsync();
                _dbContext.OrderDetails.AddRange(orderDetails.Select(x => { x.OrderId = order.Id; return x; }));
                _dbContext.Product.UpdateRange(purchasedProducts);
                var isTransactionDetailsUpdated = await UpdateTransactionDetailsAsync(purchaseViewModel.PaymentAccountId, customerId);
                if (!isTransactionDetailsUpdated)
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

            /// <summary>
            /// Records the latest transaction made by a customer.
            /// </summary>
            /// <param name="paymentAccountId">ID of payment account used in the latest transaction.</param>
            /// <param name="customerId">ID of customer who made the transaction.</param>
            /// <returns>True if the update succeeds and False otherwise.</returns>
            async Task<bool> UpdateTransactionDetailsAsync(int paymentAccountId, string customerId)
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