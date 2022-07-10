using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Implementations;

internal class OrderService : ServiceBase, IOrderService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<OrderService> _logger;
    private readonly IProductService _productService;
    private readonly ICustomerAccountService _customerAccountService;
    private readonly IPaymentService _paymentService;

    public OrderService(
        XWaveDbContext dbContext,
        ILogger<OrderService> logger,
        IAuthenticationService authenticationService,
        IProductService productService,
        ICustomerAccountService customerAccountService,
        IPaymentService paymentService) : base(dbContext)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _productService = productService;
        _customerAccountService = customerAccountService;
        _paymentService = paymentService;
    }

    public async Task<OrderDto?> FindOrderByIdAsync(string customerId, int orderId)
    {
        return (await FindAllOrdersAsync(customerId)).SingleOrDefault(o => o.Id == orderId);
    }

    public async Task<ServiceResult<int>> AddOrderAsync(
        PurchaseViewModel purchaseViewModel,
        string customerId)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            if (!await _customerAccountService.CustomerAccountExists(customerId))
            {
                return ServiceResult<int>.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = "Customer account not found"
                });
            }

            if (!await _paymentService.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId))
            {
                return ServiceResult<int>.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = "Valid payment account not found"
                });
            }

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
            {
                return ServiceResult<int>.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = $"The following products were not found: { string.Join(", ", missingProductNames) }.",
                });
            }

            var purchasedProducts = new List<Product>();
            var orderDetails = new List<OrderDetails>();
            var errors = new List<Error>();

            foreach (var productInCart in purchaseViewModel.Cart)
            {
                var product = productsToPurchase[productInCart.ProductId];
                if (product.Quantity < productInCart.Quantity)
                {
                    errors.Add(new Error
                    {
                        ErrorCode = ErrorCode.EntityInvalidState,
                        Message = $"Quantity of product {product.Name} exceeded existing stock.",
                    });
                }

                //prevent customers from ordering based on incorrect data
                if (product.Discount?.Percentage != productInCart.DisplayedDiscountPercentage)
                {
                    errors.Add(new Error
                    {
                        ErrorCode = ErrorCode.EntityInconsistentStates,
                        Message = $"Discount percentage of product {product.Name} has been changed during the transaction.",
                    });
                }

                if (product.Price != productInCart.DisplayedPrice)
                {
                    errors.Add(new Error
                    {
                        ErrorCode = ErrorCode.EntityInconsistentStates,
                        Message = $"Price of product {product.Name} has been changed during the transaction.",
                    });
                }

                // move to validate remaining products without processing any of them.
                if (errors.Count > 0) continue;

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

            if (errors.Count > 0)
            {
                return ServiceResult<int>.Failure(errors);
            }

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

            return ServiceResult<int>.Success(order.Id);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed to place order for customer ID {customerId}");
            _logger.LogError($"Exception message: {exception.Message}");

            return ServiceResult<int>.Failure(Error.Default());
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