using Xunit;
using Moq;
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
using Bogus;

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
        var customerId = new Faker().Random.Guid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);
        var result = await orderService.FindAllOrdersAsync(customerId);
        var expected = ServiceResult<int>.Failure(Error.With(ErrorCode.AuthorizationError));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task FindOrderById_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var customerId = new Faker().Random.Guid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        var result = await orderService.FindAllOrdersAsync(customerId);
        var expected = ServiceResult<int>.Failure(Error.With(ErrorCode.AuthorizationError));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);
        
        var customerId = new Faker().Random.Guid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(false);

        var result = await orderService.AddOrderAsync(It.IsAny<PurchaseViewModel>(), customerId);
        var expected = ServiceResult<int>.Failure(Error.With(
                    ErrorCode.AuthorizationError,
                    "Customer account not found"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfPaymentAccountIsInvalid()
    {
        var faker = new Faker();
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var randomPaymentAccountId = faker.Random.Number();
        var purchaseViewModel = new PurchaseViewModel() { PaymentAccountId = randomPaymentAccountId };
        var customerId = faker.Random.Guid().ToString();

        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId))
            .ReturnsAsync(true);

        _mockPaymentService
            .Setup(x => x.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId, false))
            .ReturnsAsync(false);

        var result = await orderService.AddOrderAsync(purchaseViewModel, customerId);
        var expected = ServiceResult<int>.Failure(
                Error.With(
                    ErrorCode.InvalidState,
                    "Valid payment account not found"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfPurchasedProductsNotFoundAsync()
    {
        var faker = new Faker();
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);
        var orderService = CreateTestSubject(dbContext);

        var existingProductIds = await dbContext.Product.Select(x => x.Id).ToListAsync();

        var randomNonExistentProductIds = Enumerable.Range(0, 100)
            .Select(_ => faker.Random.Number(int.MinValue, int.MaxValue))
            .Distinct()
            .Where(x => !existingProductIds.Contains(x))
            .ToList();

        var randomPaymentAccountId = faker.Random.Int();

        var itemsToPurchase = randomNonExistentProductIds
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
        var customerId = faker.Random.Guid().ToString();

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
        var expected = ServiceResult<int>.Failure(
                Error.With(
                    ErrorCode.InvalidState,
                    $"The following products were not found: {string.Join(", ", randomNonExistentProductIds)}."));

        AssertEqualServiceResults(result, expected);
    }
}