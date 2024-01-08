using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.DTOs.Customers;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Implementations;

internal class OrderService : ServiceBase, IOrderService
{
    private readonly ICustomerAccountService _customerAccountService;
    private readonly IPaymentAccountService _paymentService;
    private readonly IDiscountedProductPriceCalculator _discountedPriceCalculator;

    public OrderService(
        XWaveDbContext dbContext,
        ICustomerAccountService customerAccountService,
        IPaymentAccountService paymentService,
        IDiscountedProductPriceCalculator discountedPriceCalculator) : base(dbContext)
    {
        _customerAccountService = customerAccountService;
        _paymentService = paymentService;
        _discountedPriceCalculator = discountedPriceCalculator;
    }

    public async Task<ServiceResult<OrderDto>> FindOrderByIdAsync(string customerId, int orderId)
    {
        if (!await _customerAccountService.CustomerAccountExists(customerId))
        {
            return ServiceResult<OrderDto>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
            });
        }

        var order = await GetOrderDtosQuery(customerId).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
            });
        }

        return ServiceResult<OrderDto>.Success(order);
    }

    public async Task<ServiceResult<int>> AddOrderAsync(
        PurchaseViewModel purchaseViewModel,
        string customerId)
    {
        if (!await _customerAccountService.CustomerAccountExists(customerId))
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
                Message = "Customer account not found"
            });
        }

        if (!await _paymentService.CustomerHasPaymentAccount(
            customerId,
            purchaseViewModel.PaymentAccountId))
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = "Valid payment account not found"
            });
        }

        var order = new Order
        {
            CustomerId = customerId,
            PaymentAccountId = purchaseViewModel.PaymentAccountId,
            DeliveryAddress = purchaseViewModel.DeliveryAddress
        };

        var productsToPurchaseIds = purchaseViewModel.ProductCart.Select(p => p.ProductId);
        var productsToPurchase = await DbContext.Product
            .Include(p => p.Discount)
            .Where(p => productsToPurchaseIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var missingProductIds = productsToPurchaseIds
            .Except(productsToPurchase.Select(p => p.Key))
            .ToArray();

        if (missingProductIds.Any())
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = $"The following products were not found: { string.Join(", ", missingProductIds) }.",
            });
        }

        var purchasedProducts = new List<Product>();
        var orderDetails = new List<OrderDetails>();
        var errorMessages = new List<string>();

        foreach (var productInCart in purchaseViewModel.ProductCart)
        {
            var product = productsToPurchase[productInCart.ProductId];
            if (product.Quantity < productInCart.Quantity)
            {
                errorMessages.Add($"Quantity of product {product.Name} exceeded existing stock.");
            }

            //prevent customers from ordering based on incorrect data
            if ((product.Discount?.Percentage ?? 0) != productInCart.DisplayedDiscountPercentage)
            {
                errorMessages.Add($"Discount percentage of product {product.Name} has been changed during the transaction.");
            }

            if (product.Price != productInCart.DisplayedPrice)
            {
                errorMessages.Add($"Price of product {product.Name} has been changed during the transaction.");
            }

            // move to validate remaining products without processing any of them.
            if (errorMessages.Count > 0)
            {
                continue;
            }

            var purchasePrice = product.Discount is null
                ? product.Price
                : _discountedPriceCalculator.CalculatePriceAfterDiscount(product);

            product.Quantity -= productInCart.Quantity;
            purchasedProducts.Add(product);
            orderDetails.Add(new OrderDetails
            {
                Quantity = productInCart.Quantity,
                ProductId = productInCart.ProductId,
                PriceAtOrder = purchasePrice,
                Order = order,
            });
        }

        if (errorMessages.Count > 0)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.ConflictingState,
                Message = string.Join("\n", errorMessages)
            });
        }

        DbContext.Order.Add(order);
        DbContext.OrderDetails.AddRange(orderDetails);
        DbContext.Product.UpdateRange(purchasedProducts);

        await DbContext.SaveChangesAsync();

        return ServiceResult<int>.Success(order.Id);

    }

    public async Task<ServiceResult<IReadOnlyCollection<OrderDto>>> FindAllOrdersAsync(string customerId)
    {
        if (!await _customerAccountService.CustomerAccountExists(customerId))
        {
            return ServiceResult<IReadOnlyCollection<OrderDto>>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
                Message = "Customer account not found"
            });
        }

        var orderDtos = await GetOrderDtosQuery(customerId).ToListAsync();
        return ServiceResult<ReadOnlyCollection<OrderDto>>.Success(orderDtos.AsIReadonlyCollection());
    }

    private IQueryable<OrderDto> GetOrderDtosQuery(string customerId)
    {
        return DbContext.Order
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
                Provider = o.Payment.Provider,
                ProductPurchaseDetails = o
                    .OrderDetails
                    .Select(od => new ProductPurchaseDetailsDto
                    {
                        Quantity = od.Quantity,
                        Price = od.PriceAtOrder,
                        ProductName = od.Product.Name
                    })
            });
    }
}
