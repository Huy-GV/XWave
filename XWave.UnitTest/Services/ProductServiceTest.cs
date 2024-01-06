using FluentAssertions;
using FsCheck;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using XWave.Core.Data.Constants;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;
using System.Threading.Tasks;
using Hangfire;

namespace XWave.UnitTest.Services;

public class ProductServiceTest : BaseTest
{
    private readonly IProductManagementService _productService;
    private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient = new();
    private readonly Mock<ILogger<ProductManagementService>> _mockLog = new();
    private readonly Mock<IStaffActivityLogger> _mockStaffActivityLogger = new();
    private readonly Mock<IRoleAuthorizer> _mockRoleAuthorizer = new();
    private readonly ProductDtoMapper _productMapper = new();

    private readonly List<Discount> _testDiscounts = TestDiscountFactory.Discounts();
    private readonly List<Category> _testCategories = TestCategoryFactory.Categories();
    private readonly List<Product> _testProducts = new();

    public ProductServiceTest() : base()
    {
        var dbContext = CreateDbContext();
        dbContext.Category.AddRange(_testCategories);
        dbContext.SaveChanges();
        dbContext.Discount.AddRange(_testDiscounts);
        dbContext.SaveChanges();

        _testProducts = TestProductFactory.Products(_testCategories, _testDiscounts);
        var randomIndex = new System.Random().Next(1, int.MaxValue) % _testProducts.Count;
        _testProducts[randomIndex].IsDiscontinued = true;
        _testProducts[randomIndex].DiscontinuationDate = DateTime.Now.AddDays(-7);
        dbContext.Product.AddRange(_testProducts);
        dbContext.SaveChanges();

        _productService = new ProductManagementService(
            dbContext,
            _mockStaffActivityLogger.Object,
            _mockBackgroundJobClient.Object,
            _productMapper,
            _mockLog.Object,
            _mockRoleAuthorizer.Object);
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfOneProductIsMissing()
    {
        var nonExistentProductId = _testProducts.Sum(x => x.Id);
        var testProductIds = new List<int>(_testProducts.Select(x => x.Id)) { nonExistentProductId };
        var userId = Guid.NewGuid().ToString();

        SetUpManagerRoleCheckBypass(userId);
        var result = await _productService.DiscontinueProductAsync(
            userId,
            testProductIds.ToArray(),
            DateTime.MaxValue);

        result
            .Should()
            .BeEquivalentTo(ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Products with the following IDs not found: {string.Join(", ", new[] { nonExistentProductId })}.",
            }));
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfProductIsAlreadyDiscontinued()
    {
        var discontinuedProductId = _testProducts.First(x => x.IsDiscontinued).Id;
        var userId = Guid.NewGuid().ToString();

        SetUpManagerRoleCheckBypass(userId);
        var result = await _productService.DiscontinueProductAsync(
            userId,
            _testProducts.Select(x => x.Id).ToArray(),
            DateTime.MaxValue);

        result
            .Should()
            .BeEquivalentTo(ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = $"Products with the following IDs already discontinued: {string.Join(", ", new[] { discontinuedProductId })}."
            }));
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfScheduleIsNotFuture()
    {
        var testProductIds = _testProducts
            .Where(x => !x.IsDiscontinued)
            .Select(x => x.Id).ToArray();
        var userId = Guid.NewGuid().ToString();
        var schedule = DateTime.Now.AddDays(new System.Random().NextDouble() % (DateTime.Now - DateTime.MinValue).Days);
        SetUpManagerRoleCheckBypass(userId);
        var result = await _productService.DiscontinueProductAsync(
            userId,
            testProductIds,
            schedule);

        result
            .Should()
            .BeEquivalentTo(ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale discontinuation date must be at least 1 week in the future.",
            }));
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfScheduleIsUnderOneWeek()
    {
        var randomDay = Math.Abs(new System.Random().NextInt64()) % 6;
        var testProductIds = _testProducts
            .Where(x => !x.IsDiscontinued)
            .Select(x => x.Id).ToArray();
        var userId = Guid.NewGuid().ToString();
        var schedule = DateTime.Now.AddDays(randomDay);

        SetUpManagerRoleCheckBypass(userId);
        var result = await _productService.DiscontinueProductAsync(
            userId,
            testProductIds,
            schedule);

        result
            .Should()
            .BeEquivalentTo(ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale discontinuation date must be at least 1 week in the future.",
            }));
    }

    [Fact]
    public void UpdateProduct_ShouldFail_IfUserIsNotStaff()
    {
        Prop.ForAll(
            NonStaffRoles(),
            Arb.Default.Guid(),
            async (invalidRoles, guid) =>
        {
            var userId = guid.ToString();
            _mockRoleAuthorizer
                .Setup(x => x.GetRolesByUserId(userId).Result)
                .Returns(invalidRoles);

            var result = await _productService.UpdateProductAsync(userId, It.IsAny<int>(), It.IsAny<UpdateProductViewModel>());

            result.Error
                .Should()
                .BeEquivalentTo(new Error()
                {
                    Code = ErrorCode.AuthorizationError,
                    Message = "Only staff are authorized to modify products"
                });
        });
    }

    [Fact]
    public void UpdateProductPrice_ShouldFail_IfUserIsNotStaff()
    {
        Prop.ForAll(
            NonStaffRoles(),
            Arb.Default.Guid(),
            async (invalidRoles, guid) =>
        {
            var userId = guid.ToString();
            _mockRoleAuthorizer
                .Setup(x => x.GetRolesByUserId(userId).Result)
                .Returns(invalidRoles);
            var result = await _productService.UpdateProductPriceAsync(userId, It.IsAny<int>(), It.IsAny<UpdateProductPriceViewModel>());
            
            result.Error
                .Should()
                .BeEquivalentTo(new Error()
                {
                    Code = ErrorCode.AuthorizationError,
                    Message = "Only staff are authorized to modify products"
                });
        });
    }

    [Fact]
    public void UpdateProductStock_ShouldFail_IfUserIsNotStaff()
    {
        Prop.ForAll(
            NonStaffRoles(),
            Arb.Default.Guid(),
            async (invalidRoles, guid) =>
            {
                var userId = guid.ToString();
                _mockRoleAuthorizer
                                .Setup(x => x.GetRolesByUserId(userId).Result)
                                .Returns(invalidRoles);
                var result = await _productService.UpdateStockAsync(userId, It.IsAny<int>(), It.IsAny<uint>());
                
                result.Error
                    .Should()
                    .BeEquivalentTo(new Error()
                    {
                        Code = ErrorCode.AuthorizationError,
                        Message = "Only staff are authorized to modify products"
                    });
            });
    }

    [Fact]
    public void RestartProductSale_ShouldFail_IfUserIsNotManager()
    {
        Prop.ForAll(
            NonManagerRoles(),
            Arb.Default.Guid(),
            async (invalidRoles, guid) =>
            {
                var userId = guid.ToString();
                _mockRoleAuthorizer
                    .Setup(x => x.GetRolesByUserId(userId).Result)
                    .Returns(invalidRoles);
                var result = await _productService.RestartProductSaleAsync(userId, It.IsAny<int>(), It.IsAny<DateTime>());

                result.Error
                    .Should()
                    .BeEquivalentTo(new Error()
                    {
                        Code = ErrorCode.AuthorizationError,
                        Message = "Only staff are authorized to modify products"
                    });
            });
    }

    [Fact]
    public void UpdateProductSaleStatus_ShouldFail_IfUserIsNotStaff()
    {
    }

    private void SetUpManagerRoleCheckBypass(string userId)
    {
        _mockRoleAuthorizer.Setup(x => x.IsUserInRole(userId, "Manager").Result)
            .Returns(true);
    }

    private static Arbitrary<string[]> NonStaffRoles()
    {
        return Gen.ArrayOf(
            100,
            Gen.OneOf(new[]
            {
                Gen.Constant(RoleNames.Customer),
                Arb.Default.String().Generator,
            }))
            .Where(x => !x.Intersect(new[] { RoleNames.Staff, RoleNames.Manager })
            .Any())
        .ToArbitrary();
    }

    private static Arbitrary<string[]> NonManagerRoles()
    {
        return Gen.ArrayOf(
            100,
            Gen.OneOf(new[]
            {
                Gen.Constant(RoleNames.Staff),
                Gen.Constant(RoleNames.Customer),
                Arb.Default.String().Generator,
            }))
            .Where(x => !x.Contains(RoleNames.Manager))
        .ToArbitrary();
    }
}