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

namespace XWave.Test.Services;

public class OrderServiceTest : BaseTest
{
    private readonly IOrderService _orderService;
    private readonly Mock<ICustomerAccountService> _mockCustomerAccountService = new();
    private readonly Mock<IPaymentAccountService> _mockPaymentService = new();
    private readonly Mock<IDiscountedProductPriceCalculator> _mockDiscountCalculator = new();

    private readonly List<Product> _testProducts;

    private readonly List<Category> _testCategories;

    private readonly List<Discount> _testDiscounts;

    public OrderServiceTest()
    {
        _testCategories = TestCategoryFactory.Categories();
        _testDiscounts = TestDiscountFactory.Discounts();

        var dbContext = CreateDbContext();
        dbContext.Discount.AddRange(_testDiscounts);
        dbContext.Category.AddRange(_testCategories);
        dbContext.SaveChanges();
        _testProducts = TestProductFactory.Products(_testCategories, _testDiscounts);
        dbContext.Product.AddRange(_testProducts);
        dbContext.SaveChanges();

        _orderService = new OrderService(
            dbContext,
            _mockCustomerAccountService.Object,
            _mockPaymentService.Object,
            _mockDiscountCalculator.Object);
    }

    [Fact]
    public async Task AddOrder_ShouldFail_IfCustomerAccountDoesNotExist()
    {
        var customerId = Guid.NewGuid().ToString();
        _mockCustomerAccountService
            .Setup(x => x.CustomerAccountExists(customerId).Result)
            .Returns(false);

        var result = await _orderService.AddOrderAsync(It.IsAny<PurchaseViewModel>(), customerId);
        var expected = ServiceResult<int>.Failure(new Error()
        {
            Code = ErrorCode.AuthorizationError,
            Message = "Customer account not found"
        });

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public void AddOrder_ShouldFail_IfPaymentAccountIsInvalid()
    {
        Prop.ForAll(
            Arb.Default.Int32(),
            async (paymentAccountId) =>
        {
            var purchaseViewModel = new PurchaseViewModel() { PaymentAccountId = paymentAccountId };
            var customerId = Guid.NewGuid().ToString();
            _mockCustomerAccountService
                .Setup(x => x.CustomerAccountExists(customerId).Result)
                .Returns(true);
            _mockPaymentService
                .Setup(x => x.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId, false).Result)
                .Returns(false);

            var result = await _orderService.AddOrderAsync(purchaseViewModel, customerId);
            var expected = ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = "Valid payment account not found"
            });

            AssertEqualServiceResults(result, expected);

        }).QuickCheckThrowOnFailure();
    }

    [Fact]
    public void AddOrder_ShouldFail_IfPurchasedProductsNotFound()
    {
        var existingProductIds = _testProducts.Select(x => x.Id).ToList();
        var randomNonExistentProductIdGen = Gen
            .ArrayOf(100, Arb.Default.Int32().Generator)
            .Where(x => !existingProductIds.Intersect(x).Any())
            .Select(x => x.Distinct())
            .ToArbitrary();

        Prop.ForAll(
            randomNonExistentProductIdGen,
            Arb.Default.Int32(),
            async (randomMissingIds, paymentAccountId) =>
        {
            var itemsToPurchase = randomMissingIds
                .Select(x => new PurchaseViewModel.PurchasedItems
                {
                    ProductId = x,
                })
                .ToList();

            var purchaseViewModel = new PurchaseViewModel()
            {
                PaymentAccountId = paymentAccountId,
                ProductCart = itemsToPurchase
            };

            var customerId = Guid.NewGuid().ToString();
            _mockCustomerAccountService
                .Setup(x => x.CustomerAccountExists(customerId).Result)
                .Returns(true);
            _mockPaymentService
                .Setup(x => x.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId, false).Result)
                .Returns(true);

            var result = await _orderService.AddOrderAsync(purchaseViewModel, customerId);
            var expected = ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = $"The following products were not found: {string.Join(", ", randomMissingIds)}.",
            });

            AssertEqualServiceResults(result, expected);
        }).QuickCheckThrowOnFailure();
    }
}