using FluentAssertions;
using FsCheck;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;
using System.Threading.Tasks;
using XWave.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace XWave.UnitTest.Services;

public class OrderServiceTest : BaseTest
{
    private readonly Mock<ICustomerAccountService> _mockCustomerAccountService = new();
    private readonly Mock<IPaymentAccountService> _mockPaymentService = new();
    private readonly Mock<IDiscountedProductPriceCalculator> _mockDiscountCalculator = new();
    
    private static async Task SeedTestDataAsync(
        XWaveDbContext dbContext,
        List<Product>? testProducts = null)
    {
        if (testProducts is null)
        {
            var testDiscounts = TestDiscountFactory.Discounts();
            var testCategories = TestCategoryFactory.Categories();

            dbContext.Category.AddRange(testCategories);
            await dbContext.SaveChangesAsync();

            dbContext.Discount.AddRange(testDiscounts);
            await dbContext.SaveChangesAsync();

            testProducts = TestProductFactory.Products(testCategories, testDiscounts);
        }

        dbContext.Product.AddRange(testProducts);
        await dbContext.SaveChangesAsync();
    }

    private IOrderService CreateTestSubject(XWaveDbContext dbContext)
    {
        return new OrderService(
            dbContext,
            _mockCustomerAccountService.Object,
            _mockPaymentService.Object,
            _mockDiscountCalculator.Object);
    }

    [Fact]
    public async Task FindAllOrders_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        var customerId = Guid.NewGuid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        using var dbContext = CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);
        var result = await orderService.FindAllOrdersAsync(customerId);
        var expected = ServiceResult<int>.Failure(new Error()
        {
            Code = ErrorCode.AuthorizationError,
            Message = "Customer account not found"
        });

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task FindOrderById_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        using var dbContext = CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var customerId = Guid.NewGuid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        var result = await orderService.FindAllOrdersAsync(customerId);
        var expected = ServiceResult<int>.Failure(new Error()
        {
            Code = ErrorCode.AuthorizationError,
            Message = "Customer account not found"
        });

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        using var dbContext = CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);
        
        var customerId = Guid.NewGuid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        var result = await orderService.AddOrderAsync(It.IsAny<PurchaseViewModel>(), customerId);
        var expected = ServiceResult<int>.Failure(new Error()
        {
            Code = ErrorCode.AuthorizationError,
            Message = "Customer account not found"
        });

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfPaymentAccountIsInvalid()
    {
        using var dbContext = CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var randomPaymentAccountId = (new System.Random()).Next();
        var purchaseViewModel = new PurchaseViewModel() { PaymentAccountId = randomPaymentAccountId };
        var customerId = Guid.NewGuid().ToString();

        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(true);

        _mockPaymentService
            .Setup(x => x.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId, false))
            .ReturnsAsync(false);

        var result = await orderService.AddOrderAsync(purchaseViewModel, customerId);
        var expected = ServiceResult<int>.Failure(new Error
        {
            Code = ErrorCode.InvalidState,
            Message = "Valid payment account not found"
        });

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfPurchasedProductsNotFoundAsync()
    {
        using var dbContext = CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var existingProductIds = await dbContext.Product.Select(x => x.Id).ToListAsync();
        var randomNonExistentProductIdGen = Gen
            .ArrayOf(100, Arb.Default.Int32().Generator)
            .Where(x => !existingProductIds.Intersect(x).Any())
            .Select(x => x.Distinct());

        var randomPaymentAccountId = (new System.Random()).Next();
        var randomMissingIds = Gen
            .ArrayOf(100, Arb.Default.Int32().Generator)
            .Where(x => !existingProductIds.Intersect(x).Any())
            .Select(x => x.Distinct())
            .Sample(1, 1)
            .Head;

        var itemsToPurchase = randomMissingIds
            .Select(x => new PurchaseViewModel.PurchasedItems
            {
                ProductId = x,
            })
            .ToList();

        var purchaseViewModel = new PurchaseViewModel()
        {
            PaymentAccountId = randomPaymentAccountId,
            ProductCart = itemsToPurchase
        };

        var includeExpiredAccounts = false;
        var customerId = Guid.NewGuid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(true);

        _mockPaymentService
            .Setup(x => x.CustomerHasPaymentAccount(
                customerId, 
                purchaseViewModel.PaymentAccountId,
                includeExpiredAccounts))
            .ReturnsAsync(true);

        var result = await orderService.AddOrderAsync(purchaseViewModel, customerId);
        var expected = ServiceResult<int>.Failure(new Error
        {
            Code = ErrorCode.InvalidState,
            Message = $"The following products were not found: {string.Join(", ", randomMissingIds)}.",
        });

        AssertEqualServiceResults(result, expected);
    }
}