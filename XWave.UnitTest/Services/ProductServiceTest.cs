using FluentAssertions;
using Microsoft.Extensions.Logging;
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
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;
using System.Threading.Tasks;
using Hangfire;
using XWave.Core.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace XWave.UnitTest.Services;

public class ProductServiceTest : BaseTest
{
    private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient = new();
    private readonly Mock<ILogger<ProductManagementService>> _mockLog = new();
    private readonly Mock<IStaffActivityLogger> _mockStaffActivityLogger = new();
    private readonly Mock<IRoleAuthorizer> _mockRoleAuthorizer = new();
    private readonly ProductDtoMapper _productMapper = new();

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

    private IProductManagementService CreateTestSubject(XWaveDbContext dbContext)
    {
        return new ProductManagementService(
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
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var faker = new Faker();
        var testProductIds = await dbContext.Product.Select(x => x.Id).ToListAsync();
        var nonExistentProductIds = Enumerable
            .Range(0, 100)
            .Select(_ => faker.Random.Number(int.MinValue, int.MaxValue))
            .Distinct()
            .Where(x => !testProductIds.Contains(x))
            .ToList();

        var userId = faker.Random.Guid().ToString();

        SetUpManagerRoleCheckBypass(userId);
        var result = await productManagementService.DiscontinueProductAsync(
            userId,
            testProductIds.Concat(nonExistentProductIds),
            DateTime.MaxValue);

        var expected = ServiceResult<int>.Failure(
                Error.With(ErrorCode.EntityNotFound, 
                $"Products with the following IDs not found: {string.Join(", ", nonExistentProductIds)}."));

        AssertEqualServiceResults(expected, result);
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfProductIsAlreadyDiscontinued()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var testProducts = await dbContext.Product.Where(x => !x.IsDiscontinued).ToListAsync();
        var discontinuedProductIds = await dbContext.Product
            .Where(x => x.IsDiscontinued)
            .Select(x => x.Id)
            .ToListAsync();

        var userId = new Faker().Random.Guid().ToString();

        SetUpManagerRoleCheckBypass(userId);
        var result = await productManagementService.DiscontinueProductAsync(
            userId,
            testProducts.Select(x => x.Id).Concat(discontinuedProductIds),
            DateTime.MaxValue);

        var expected = ServiceResult<int>.Failure(
                Error.With(ErrorCode.InvalidState,
                $"Products with the following IDs already discontinued: {string.Join(", ", discontinuedProductIds)}."));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldFail_IfScheduleIsUnderOneWeek()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var testProducts = await dbContext.Product.ToListAsync();

        var faker = new Faker();

        // either in the past or less than 1 week in the future
        var randomDayOffset = faker.Random.Number(-1000, 6);
        var testProductIds = testProducts
            .Where(x => !x.IsDiscontinued)
            .Select(x => x.Id).ToArray();

        var userId = new Faker().Random.Guid().ToString();
        var schedule = DateTime.Now.AddDays(randomDayOffset);

        SetUpManagerRoleCheckBypass(userId);
        var result = await productManagementService.DiscontinueProductAsync(
            userId,
            testProductIds,
            schedule);

        var expected = ServiceResult.Failure(
                Error.With(ErrorCode.InvalidArgument,
                "Scheduled sale discontinuation date must be at least 1 week in the future."));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task UpdateProduct_ShouldFail_IfUserIsNotStaffAsync()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var invalidRoles = NonStaffRoles();
        var userId = new Faker().Random.Guid().ToString();

        _mockRoleAuthorizer
            .Setup(x => x.GetRolesByUserId(userId))
            .ReturnsAsync(invalidRoles);

        var result = await productManagementService.UpdateProductAsync(
            userId,
            It.IsAny<int>(),
            It.IsAny<UpdateProductViewModel>());

        var expected = ServiceResult.Failure(Error.With(
            ErrorCode.AuthorizationError,
            "Only staff are authorized to modify products"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task UpdateProductPrice_ShouldFail_IfUserIsNotStaffAsync()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var invalidRoles = NonStaffRoles();
        var userId = new Faker().Random.Guid().ToString();
        _mockRoleAuthorizer
            .Setup(x => x.GetRolesByUserId(userId))
            .ReturnsAsync(invalidRoles);

        var result = await productManagementService.UpdateProductPriceAsync(
            userId,
            It.IsAny<int>(),
            It.IsAny<UpdateProductPriceViewModel>());

        var expected = ServiceResult.Failure(Error.With(
            ErrorCode.AuthorizationError,
            "Only staff are authorized to modify products"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task UpdateProductStock_ShouldFail_IfUserIsNotStaffAsync()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var invalidRoles = NonStaffRoles();
        var userId = new Faker().Random.Guid().ToString();

        _mockRoleAuthorizer
            .Setup(x => x.GetRolesByUserId(userId))
            .ReturnsAsync(invalidRoles);

        var result = await productManagementService.UpdateStockAsync(
            userId,
            It.IsAny<int>(),
            It.IsAny<uint>());

        var expected = ServiceResult.Failure(Error.With(
            ErrorCode.AuthorizationError,
            "Only staff are authorized to modify products"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public async Task RestartProductSale_ShouldFail_IfUserIsNotManagerAsync()
    {
        await using var dbContext = await CreateDbContext();
        await SeedTestDataAsync(dbContext);

        var productManagementService = CreateTestSubject(dbContext);
        var invalidRoles = NonStaffRoles();
        var userId = new Faker().Random.Guid().ToString();
        _mockRoleAuthorizer
            .Setup(x => x.GetRolesByUserId(userId))
            .ReturnsAsync(invalidRoles);

        var result = await productManagementService.RestartProductSaleAsync(
            userId,
            It.IsAny<int>(),
            It.IsAny<DateTime>());

        var expected = ServiceResult.Failure(Error.With(
            ErrorCode.AuthorizationError,
            "Only staff are authorized to modify products"));

        AssertEqualServiceResults(result, expected);
    }

    [Fact]
    public void UpdateProductSaleStatus_ShouldFail_IfUserIsNotStaff()
    {
    }

    private void SetUpManagerRoleCheckBypass(string userId)
    {
        _mockRoleAuthorizer
            .Setup(x => x.IsUserInRole(userId, "Manager"))
            .ReturnsAsync(true);
    }

    private static Faker<string[]> NonStaffRoles()
    {
        var faker = new Faker<string[]>()
            .CustomInstantiator(f => f.Random
                    .ArrayElements(new[]  { "Customer", f.Random.Word() })
                    .Where(role => role is not "Staff" and not "Manager")
                    .ToArray());

        return faker;
    }
}