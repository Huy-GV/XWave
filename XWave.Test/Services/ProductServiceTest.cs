﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace XWave.Test.Services
{
    [TestClass]
    public class ProductServiceTest : BaseTest
    {
        private readonly IProductService _productService;
        private readonly Mock<IBackgroundJobService> _mockBackgroundJobService = new();
        private readonly Mock<ILogger<ProductService>> _mockLog = new();
        private readonly Mock<IActivityService> _mockActivityService = new();
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
            var randomIndex = new Random().Next(1, int.MaxValue) % _testProducts.Count;
            _testProducts[randomIndex].IsDiscontinued = true;
            _testProducts[randomIndex].DiscontinuationDate = DateTime.Now.AddDays(-7);
            dbContext.Product.AddRange(_testProducts);
            dbContext.SaveChanges();

            _productService = new ProductService(
                dbContext,
                _mockActivityService.Object,
                _mockBackgroundJobService.Object,
                _productMapper,
                _mockLog.Object);
        }

        [TestMethod]
        public void DiscontinueProductShouldFailIfOneProductIsMissing()
        {
            var nonExistentProductId = _testProducts.Sum(x => x.Id);
            var testProductIds = new List<int>(_testProducts.Select(x => x.Id)) { nonExistentProductId };
            _productService.DiscontinueProductAsync(
                new Guid().ToString(),
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
        public void DiscontinueProductShouldFailIfProductIsAlreadyDiscontinued()
        {
            var discontinuedProductId = _testProducts.First(x => x.IsDiscontinued).Id;
            _productService.DiscontinueProductAsync(
                new Guid().ToString(),
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
        public void DiscontinueProductShouldFailIfScheduleIsNotFuture()
        {
            var testProductIds = _testProducts
                .Where(x => !x.IsDiscontinued)
                .Select(x => x.Id).ToArray();
            _productService.DiscontinueProductAsync(
                new Guid().ToString(),
                testProductIds,
                DateTime.Now.AddDays(-10))
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.InvalidUserRequest,
                    Message = "Scheduled sale discontinuation date must be at least 1 week in the future.",
                }));
        }

        [TestMethod]
        public void DiscontinueProductShouldFailIfScheduleIsUnderOneWeek()
        {
            var randomDay = Math.Abs(new Random().NextInt64()) % 6;
            var testProductIds = _testProducts
                .Where(x => !x.IsDiscontinued)
                .Select(x => x.Id).ToArray();
            _productService.DiscontinueProductAsync(
                new Guid().ToString(),
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
    }
}