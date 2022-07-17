using FluentAssertions;
using FsCheck;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using XWave.Test.Generators;

namespace XWave.Test.Services
{
    [TestClass]
    public class ProductServiceTest : BaseTest
    {
        private readonly IProductService _productService;
        private readonly Mock<IBackgroundJobService> _mockBackgroundJobService = new();
        private readonly Mock<ILogger<ProductService>> _mockLog = new();
        private readonly Mock<IActivityService> _mockActivityService = new();
        private readonly Mock<IAuthorizationService> _mockAuthorizationService = new();
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

            _productService = new ProductService(
                dbContext,
                _mockActivityService.Object,
                _mockBackgroundJobService.Object,
                _productMapper,
                _mockLog.Object,
                _mockAuthorizationService.Object);
        }

        [TestMethod]
        public void DiscontinueProduct_ShouldFail_IfOneProductIsMissing()
        {
            var nonExistentProductId = _testProducts.Sum(x => x.Id);
            var testProductIds = new List<int>(_testProducts.Select(x => x.Id)) { nonExistentProductId };
            var userId = Guid.NewGuid().ToString();

            SetUpManagerRoleCheckBypass(userId);
            _productService.DiscontinueProductAsync(
                userId,
                testProductIds.ToArray(),
                DateTime.MaxValue)
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = $"Products with the following IDs not found: {string.Join(", ", new[] { nonExistentProductId })}.",
                }));
        }

        [TestMethod]
        public void DiscontinueProduct_ShouldFail_IfProductIsAlreadyDiscontinued()
        {
            var discontinuedProductId = _testProducts.First(x => x.IsDiscontinued).Id;
            var userId = Guid.NewGuid().ToString();

            SetUpManagerRoleCheckBypass(userId);
            _productService.DiscontinueProductAsync(
                userId,
                _testProducts.Select(x => x.Id).ToArray(),
                DateTime.MaxValue)
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityInvalidState,
                    Message = $"Products with the following IDs already discontinued: {string.Join(", ", new[] { discontinuedProductId })}."
                }));
        }

        [TestMethod]
        public void DiscontinueProduct_ShouldFail_IfScheduleIsNotFuture()
        {
            var testProductIds = _testProducts
                .Where(x => !x.IsDiscontinued)
                .Select(x => x.Id).ToArray();
            var userId = Guid.NewGuid().ToString();
            var schedule = new System.Random().NextDouble() % (DateTime.Now - DateTime.MinValue).Days;
            SetUpManagerRoleCheckBypass(userId);
            _productService.DiscontinueProductAsync(
                userId,
                testProductIds,
                DateTime.Now.AddDays(schedule))
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.InvalidUserRequest,
                    Message = "Scheduled sale discontinuation date must be at least 1 week in the future.",
                }));
        }

        [TestMethod]
        public void DiscontinueProduct_ShouldFail_IfScheduleIsUnderOneWeek()
        {
            var randomDay = Math.Abs(new System.Random().NextInt64()) % 6;
            var testProductIds = _testProducts
                .Where(x => !x.IsDiscontinued)
                .Select(x => x.Id).ToArray();
            var userId = Guid.NewGuid().ToString();

            SetUpManagerRoleCheckBypass(userId);
            _productService.DiscontinueProductAsync(
                userId,
                testProductIds,
                DateTime.Now.AddDays(randomDay))
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.InvalidUserRequest,
                    Message = "Scheduled sale discontinuation date must be at least 1 week in the future.",
                }));
        }

        [TestMethod]
        public void UpdateProduct_ShouldFail_IfUserIsNotStaff()
        {
            Prop.ForAll(
                NonStaffRoles(),
                Arb.Default.Guid(),
                (invalidRoles, guid) =>
            {
                var userId = guid.ToString();
                _mockAuthorizationService
                    .Setup(x => x.GetRolesByUserId(userId).Result)
                    .Returns(invalidRoles);

                _productService.UpdateProductAsync(userId, It.IsAny<int>(), It.IsAny<ProductViewModel>())
                    .Result.Errors.Single()
                    .Should()
                    .BeEquivalentTo(new Error()
                    {
                        ErrorCode = ErrorCode.InvalidUserRequest,
                        Message = "Only staff are authorized to modify products"
                    });
            });
        }

        [TestMethod]
        public void UpdateProductPrice_ShouldFail_IfUserIsNotStaff()
        {
            Prop.ForAll(
                NonStaffRoles(),
                Arb.Default.Guid(),
                (invalidRoles, guid) =>
            {
                var userId = guid.ToString();
                _mockAuthorizationService
                                .Setup(x => x.GetRolesByUserId(userId).Result)
                                .Returns(invalidRoles);
                _productService.UpdateProductPriceAsync(userId, It.IsAny<int>(), It.IsAny<uint>())
                    .Result.Errors.Single()
                    .Should()
                    .BeEquivalentTo(new Error()
                    {
                        ErrorCode = ErrorCode.InvalidUserRequest,
                        Message = "Only staff are authorized to modify products"
                    });
            });
        }

        [TestMethod]
        public void UpdateProductStock_ShouldFail_IfUserIsNotStaff()
        {
            Prop.ForAll(
                NonStaffRoles(),
                Arb.Default.Guid(),
                (invalidRoles, guid) =>
                {
                    var userId = guid.ToString();
                    _mockAuthorizationService
                                    .Setup(x => x.GetRolesByUserId(userId).Result)
                                    .Returns(invalidRoles);
                    _productService.UpdateStockAsync(userId, It.IsAny<int>(), It.IsAny<uint>())
                        .Result.Errors.Single()
                        .Should()
                        .BeEquivalentTo(new Error()
                        {
                            ErrorCode = ErrorCode.InvalidUserRequest,
                            Message = "Only staff are authorized to modify products"
                        });
                });
        }

        [TestMethod]
        public void RestartProductSale_ShouldFail_IfUserIsNotManager()
        {
            Prop.ForAll(
                NonManagerRoles(),
                Arb.Default.Guid(),
                (invalidRoles, guid) =>
                {
                    var userId = guid.ToString();
                    _mockAuthorizationService
                        .Setup(x => x.GetRolesByUserId(userId).Result)
                        .Returns(invalidRoles);
                    _productService.RestartProductSaleAsync(userId, It.IsAny<int>(), It.IsAny<DateTime>())
                        .Result.Errors.Single()
                        .Should()
                        .BeEquivalentTo(new Error()
                        {
                            ErrorCode = ErrorCode.InvalidUserRequest,
                            Message = "Only staff are authorized to modify products"
                        });
                });
        }

        public void UpdateProductSaleStatus_ShouldFail_IfUserIsNotStaff()
        {
        }

        private void SetUpManagerRoleCheckBypass(string userId)
        {
            _mockAuthorizationService.Setup(x => x.IsUserInRole(userId, "Manager").Result)
                .Returns(true);
        }

        private static Arbitrary<string[]> NonStaffRoles()
        {
            return Gen.ArrayOf(
                100,
                Gen.OneOf(new[]
                {
                    Gen.Constant(Roles.Customer),
                    Arb.Default.String().Generator,
                }))
                .Where(x => !x.Intersect(new[] { Roles.Staff, Roles.Manager })
                .Any())
            .ToArbitrary();
        }

        private static Arbitrary<string[]> NonManagerRoles()
        {
            return Gen.ArrayOf(
                100,
                Gen.OneOf(new[]
                {
                    Gen.Constant(Roles.Staff),
                    Gen.Constant(Roles.Customer),
                    Arb.Default.String().Generator,
                }))
                .Where(x => !x.Contains(Roles.Manager))
            .ToArbitrary();
        }
    }
}