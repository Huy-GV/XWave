using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.Services.ResultTemplate;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Implementations;

internal class OrderService : ServiceBase, IOrderService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<OrderService> _logger;
    private readonly IProductService _productService;

    public OrderService(
        XWaveDbContext dbContext,
        ILogger<OrderService> logger,
        IAuthenticationService authenticationService,
        IProductService productService) : base(dbContext)
    {
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
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            if (!await DbContext.CustomerAccount.AnyAsync(c => c.CustomerId == customerId) ||
                !await _authenticationService.UserExists(customerId))
                return (ServiceResult.Failure("Customer account not found."), null);

            var paymentAccount = await DbContext.PaymentAccount.FindAsync(purchaseViewModel.PaymentAccountId);
            if (paymentAccount == null) return (ServiceResult.Failure("Payment account not found."), null);

            if (paymentAccount.ExpiryDate > DateTime.Now)
                return (ServiceResult.Failure("Could not proceed because selected payment account expired."), null);

            var order = new Order
            {
                CustomerId = customerId,
                PaymentAccountId = purchaseViewModel.PaymentAccountId,
                DeliveryAddress = purchaseViewModel.DeliveryAddress
            };

            var productIdsToPurchase = purchaseViewModel.Cart.Select(p => p.ProductId);
            var productsToPurchase = await DbContext.Product
                .Include(p => p.Discount)
                .Where(p => productIdsToPurchase.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var missingProductNames = productIdsToPurchase
                .Except(productsToPurchase.Select(p => p.Key))
                .ToArray();

            if (missingProductNames.Any())
                return (
                    ServiceResult.Failure(
                        $"Error: the following products were not found: {string.Join(", ", missingProductNames)}."),
                    null);

            var purchasedProducts = new List<Product>();
            var orderDetails = new List<OrderDetails>();
            var errorMessages = new List<string>();

            foreach (var productInCart in purchaseViewModel.Cart)
            {
                var product = productsToPurchase[productInCart.ProductId];
                if (product.Quantity < productInCart.Quantity)
                    errorMessages.Add($"Quantity of product named {product.Name} exceeded existing stock.");

                //prevent customers from ordering based on incorrect data
                if (product.Discount?.Percentage != productInCart.DisplayedDiscountPercentage)
                    errorMessages.Add(
                        $"Discount percentage has been changed during the transaction. Please view the latest price for the item {product.Name}.");

                if (product.Price != productInCart.DisplayedPrice)
                    errorMessages.Add(
                        $"Price has been changed during the transaction. Please view the latest price for the item {product.Name}.");

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
                    PriceAtOrder = purchasePrice
                });
            }

            if (errorMessages.Any()) return (ServiceResult.Failure(errorMessages.ToArray()), null);

            DbContext.Order.Add(order);
            await DbContext.SaveChangesAsync();
            DbContext.OrderDetails.AddRange(orderDetails.Select(x =>
            {
                x.OrderId = order.Id;
                return x;
            }));
            DbContext.Product.UpdateRange(purchasedProducts);

            await DbContext.SaveChangesAsync();
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
        return await DbContext.Order
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Include(o => o.Payment)
            .IgnoreQueryFilters()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.Date,
                AccountNo = o.Payment.AccountNumber,
                Provider = o.Payment.AccountNumber,
                ProductPurchaseDetails = o
                    .OrderDetails
                    .Select(od => new ProductPurchaseDetailsDto
                    {
                        Quantity = od.Quantity,
                        Price = od.PriceAtOrder,
                        ProductName = od.Product.Name
                    })
            })
            .ToListAsync();
    }
}