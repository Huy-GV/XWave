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
        private readonly IProductService _productService;
        private readonly IAuthenticationService _authenticationService;

        public OrderService(
            XWaveDbContext dbContext,
            ILogger<OrderService> logger,
            IAuthenticationService authenticationService,
            IProductService productService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _authenticationService = authenticationService;
            _productService = productService;
        }

        public async Task<OrderDto?> FindOrderByIdAsync(string customerId, int orderId)
        {
            return (await FindAllOrdersAsync(customerId)).SingleOrDefault(o => o.Id == orderId);
        }

        public async Task<(ServiceResult, int? OrderId)> AddOrderAsync(
            PurchaseViewModel purchaseViewModel,
            string customerId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (!await _dbContext.CustomerAccount.AnyAsync(c => c.CustomerId == customerId) ||
                    !await _authenticationService.UserExists(customerId))
                {
                    return (ServiceResult.Failure("Customer account not found."), null);
                }

                var paymentAccount = await _dbContext.PaymentAccount.FindAsync(purchaseViewModel.PaymentAccountId);
                if (paymentAccount == null)
                {
                    return (ServiceResult.Failure("Payment account not found."), null);
                }

                if (paymentAccount.ExpiryDate > DateTime.Now)
                {
                    return (ServiceResult.Failure("Could not proceed because selected payment account expired."), null);
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

                var missingProductNames = productIdsToPurchase
                    .Except(productsToPurchase.Select(p => p.Key))
                    .ToArray();
                
                if (missingProductNames.Any())
                {
                    return (ServiceResult.Failure($"Error: the following products were not found: {string.Join(", ", missingProductNames)}."), null);
                }

                var purchasedProducts = new List<Product>();
                var orderDetails = new List<OrderDetails>();
                var errorMessages = new List<string>();

                foreach (var productInCart in purchaseViewModel.Cart)
                {
                    var product = productsToPurchase[productInCart.ProductId];
                    if (product.Quantity < productInCart.Quantity)
                    {
                        errorMessages.Add($"Quantity of product named {product.Name} exceeded existing stock.");
                    }

                    //prevent customers from ordering based on incorrect data
                    if (product.Discount?.Percentage != productInCart.DisplayedDiscountPercentage)
                    {
                        errorMessages.Add($"Discount percentage has been changed during the transaction. Please view the latest price for the item {product.Name}.");
                    }

                    if (product.Price != productInCart.DisplayedPrice)
                    {
                        errorMessages.Add($"Price has been changed during the transaction. Please view the latest price for the item {product.Name}.");
                    }

                    // move to validate remaining products without processing any of them.
                    if (errorMessages.Any()) continue;

                    var purchasePrice = product.Discount == null
                        ? product.Price
                        : _productService.CalculateDiscountedPrice(product);

                    product.Quantity -= productInCart.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetails
                    {
                        Quantity = productInCart.Quantity,
                        ProductId = productInCart.ProductId,
                        PriceAtOrder = purchasePrice,
                    });
                }

                if (errorMessages.Any())
                {
                    return (ServiceResult.Failure(errorMessages.ToArray()), null);
                }

                _dbContext.Order.Add(order);
                await _dbContext.SaveChangesAsync();
                _dbContext.OrderDetails.AddRange(orderDetails.Select(x => { x.OrderId = order.Id; return x; }));
                _dbContext.Product.UpdateRange(purchasedProducts);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (ServiceResult.Success(), order.Id);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Failed to place order for customer ID {customerId}");
                _logger.LogError($"Exception message: {exception.Message}");

                return (ServiceResult.InternalFailure(), null);
            }
        }

        public async Task<IEnumerable<OrderDto>> FindAllOrdersAsync(string customerId)
        {
            return await _dbContext.Order
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
                    Provider = o.Payment.AccountNumber,
                    ProductPurchaseDetails = o
                        .OrderDetails
                        .Select(od => new ProductPurchaseDetailsDto()
                        {
                            Quantity = od.Quantity,
                            Price = od.PriceAtOrder,
                            ProductName = od.Product.Name
                        })
                })
                .ToListAsync();
        }
    }
}