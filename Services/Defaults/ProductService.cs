using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Utils;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;
using XWave.Extensions;

namespace XWave.Services.Defaults
{
    public class ProductService : ServiceBase, IProductService
    {
        private readonly IActivityService _activityService;
        private readonly ProductDtoMapper _productHelper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            XWaveDbContext dbContext,
            IActivityService activityService,
            ProductDtoMapper productHelper,
            ILogger<ProductService> logger) : base(dbContext)
        {
            _productHelper = productHelper;
            _activityService = activityService;
            _logger = logger;
        }

        public async Task<(ServiceResult, int? ProductId)> AddProductAsync(string staffId, ProductViewModel productViewModel)
        {
            try
            {
                _logger.LogInformation($"User with ID {staffId} is attempting to add product named {productViewModel.Name}");
                if (!await DbContext.Category.AnyAsync(c => c.Id == productViewModel.CategoryId))
                {
                    return (ServiceResult.Failure("Category not found"), null);
                }

                var newProduct = new Product();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productViewModel);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    staffId,
                    OperationType.Create,
                    $"added product named {newProduct.Name} and priced ${newProduct.Price}");

                return (ServiceResult.Success(), newProduct.Id);
            }
            catch (Exception ex)
            {
                return (ServiceResult.InternalFailure(), null);
            }
        }

        public async Task<ServiceResult> DeleteProductAsync(int productId, string managerId)
        {
            try
            {
                var product = await DbContext.Product.FindAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure($"Product with ID {productId} not found");
                }

                DbContext.Product.Update(product);
                product.SoftDelete();
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    managerId,
                    OperationType.Modify,
                    $"deleted product named {product.Name}, ID = {product.Id} at {product.DeleteDate}.");

                return ServiceResult.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}.");
                return ServiceResult.InternalFailure();
            }
        }

        public Task<IEnumerable<ProductDto>> FindAllProductsForCustomers()
        {
            var productDtos = DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Where(p => !p.IsDiscontinued)
                .AsEnumerable()
                .Select(p => p.Discount == null
                    ? ProductDtoMapper.MapCustomerProductDto(p)
                    : ProductDtoMapper.MapCustomerProductDto(p, CalculateDiscountedPrice(p)));

            return Task.FromResult(productDtos);
        }

        public Task<IEnumerable<DetailedProductDto>> FindAllProductsForStaff(bool includeDiscontinuedProducts)
        {
            var products = DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .AsEnumerable()
                .Where(p => includeDiscontinuedProducts || !p.IsDiscontinued);

            return Task.FromResult(products
                .Select(p => ProductDtoMapper.MapDetailedProductDto(p))
                .AsEnumerable());
        }

        public async Task<ProductDto?> FindProductByIdForCustomers(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return null;
            }

            return product.Discount == null
                ? ProductDtoMapper.MapCustomerProductDto(product)
                : ProductDtoMapper.MapCustomerProductDto(product, CalculateDiscountedPrice(product));
        }

        public async Task<DetailedProductDto?> FindProductByIdForStaff(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : ProductDtoMapper.MapDetailedProductDto(product);
        }

        public async Task<ServiceResult> UpdateProductAsync(
            string staffId,
            int productId,
            ProductViewModel updatedProductViewModel)
        {
            try
            {
                _logger.LogInformation($"User with ID {staffId} is attempting to update product ID {productId}");
                var product = await DbContext.Product.FindAsync(productId);
                if (product == null)
                {
                    return ServiceResult.Failure($"Product with ID {productId} not found");
                }

                if (product.IsDiscontinued || product.IsDeleted)
                {
                    return ServiceResult.Failure($"Product has been discontinued or removed.");
                }

                var entry = DbContext.Update(product);
                entry.CurrentValues.SetValues(updatedProductViewModel);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    staffId,
                    OperationType.Modify,
                    $"updated general information of product named {product.Name}.");
                return ServiceResult.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}.");
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<ServiceResult> UpdateStockAsync(string staffId, int productId, uint updatedStock)
        {
            var product = await DbContext.Product.FindAsync(productId);
            if (product == null)
            {
                return ServiceResult.Failure($"Failed to update stock of product with ID {productId} because it was not found.");
            }

            try
            {
                var quantityBeforeRestock = product.Quantity;
                product.Quantity = updatedStock;
                product.LatestRestock = DateTime.Now;
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    staffId,
                    OperationType.Modify,
                    $"updated stock of product named {product.Name} (from {quantityBeforeRestock} to {updatedStock}.");

                return ServiceResult.Success();
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"Failed to update stock of product with ID {product.Id}.");
                _logger.LogDebug($"Exception: {exception.Message}");
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice)
        {
            var product = await DbContext.Product.FindAsync(productId);
            if (product == null)
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} because it was not found.");
            }

            try
            {
                var formerPrice = product.Price;
                product.Price = updatedPrice;
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    staffId,
                    OperationType.Modify,
                    $"updated price of product named {product.Name} (from {formerPrice} to {updatedPrice}.");

                return ServiceResult.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to update general information of product with ID {productId}.");
                _logger.LogDebug($"Exception message: {exception.Message}");
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice, DateTime updateSchedule)
        {
            if (!await DbContext.Product.AnyAsync(p => p.Id == productId))
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} because it was not found.");
            }

            var now = DateTime.Now;
            if (updateSchedule < now)
            {
                return ServiceResult.Failure("Scheduled update date must be in the future.");
            }

            if (updateSchedule < now.AddDays(1))
            {
                return ServiceResult.Failure("Scheduled update date must be at least 1 day in the future.");
            }

            BackgroundJob.Schedule(
                methodCall: () => ScheduledUpdateProductPrice(staffId, productId, updatedPrice),
                delay: updateSchedule - now);

            await _activityService.LogActivityAsync<Product>(
                staffId,
                OperationType.Modify,
                $"scheduled a price update (to ${updatedPrice}) for product ID {productId} at {updateSchedule}.");

            return ServiceResult.Success();
        }

        public async Task ScheduledUpdateProductPrice(string staffId, int productId, uint updatedPrice)
        {
            var product = await DbContext.Product.FindAsync(productId);
            if (product != null)
            {
                product.Price = updatedPrice;
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Product>(
                    staffId,
                    OperationType.Modify,
                    $"carried out a scheduled change in the price of product ID = {productId}. The new price is {updatedPrice}.");
            }
        }

        /// <inheritdoc/>
        /// <remarks>This method will return a failed result if one of the passed product IDs belongs to no product or if one of the products is already discontinued. However, it does not check for products that are already scheduled for discontinuation.
        /// </remarks>
        public async Task<ServiceResult> DiscontinueProductAsync(string managerId, int[] productIds, DateTime updateSchedule)
        {
            var productsToDiscontinue = await DbContext.Product
                .Where(product => productIds.Contains(product.Id))
                .ToArrayAsync();

            var missingProducts = productIds
                .Except(productsToDiscontinue.Select(p => p.Id))
                .ToArray();
            
            if (missingProducts.Any())
            {
                return ServiceResult.Failure($"Failed to discontinue product with the following IDs: {string.Join(", ", missingProducts)} because they were not found.");
            }

            var discontinuedProducts = productsToDiscontinue
                .Where(p => p.IsDiscontinued)
                .Select(p => p.Id)
                .ToArray();
            
            if (discontinuedProducts.Any())
            {
                return ServiceResult.Failure($"Failed to discontinue of product with the following IDs: {string.Join(", ", discontinuedProducts)} because they were already discontinued.");
            }

            var now = DateTime.Now;
            if (updateSchedule < now)
            {
                return ServiceResult.Failure("Scheduled sale discontinuation date must be in the future.");
            }

            if (updateSchedule < now.AddDays(7))
            {
                return ServiceResult.Failure("Scheduled sale discontinuation date must be at least 1 week in the future.");
            }

            BackgroundJob.Schedule(
                methodCall: () => UpdateProductSaleStatusByScheduleAsync(productIds, false, updateSchedule),
                delay: updateSchedule - now);

            await _activityService.LogActivityAsync<Product>(
                managerId,
                OperationType.Modify,
                $"discontinued sale of product with IDs {string.Join(", ", productIds)}, effective at {updateSchedule:d MMMM yyyy}.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RestartProductSaleAsync(string managerId, int productId, DateTime updateSchedule)
        {
            if (!await DbContext.Product.AnyAsync(p => p.Id == productId))
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} because it was not found.");
            }

            var now = DateTime.Now;
            if (updateSchedule < now)
            {
                return ServiceResult.Failure("Scheduled sale restart date must be in the future.");
            }

            if (updateSchedule < now.AddDays(7))
            {
                return ServiceResult.Failure("Scheduled sale restart date must be at least 1 week in the future.");
            }

            BackgroundJob.Schedule(
                methodCall: () => UpdateProductSaleStatusByScheduleAsync(productId, false, updateSchedule),
                delay: updateSchedule - now);

            await _activityService.LogActivityAsync<Product>(
                managerId,
                OperationType.Modify,
                $"restarted sale of product ID {productId}, effective {updateSchedule:d MMMM yyyy}.");

            return ServiceResult.Success();
        }

        public async Task UpdateProductSaleStatusByScheduleAsync(int productId, bool isDiscontinued, DateTime updateSchedule)
        {
            var product = await DbContext.Product.FindAsync(productId);
            if (product != null)
            {
                product.IsDiscontinued = isDiscontinued;
                product.DiscontinuationDate = isDiscontinued ? updateSchedule : null;
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateProductSaleStatusByScheduleAsync(int[] productIds, bool isDiscontinued, DateTime updateSchedule)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var productId in productIds)
                {
                    var product = await DbContext.Product.FindAsync(productId);
                    if (product == null) continue;
                    product.IsDiscontinued = isDiscontinued;
                    product.DiscontinuationDate = isDiscontinued ? updateSchedule : null;
                    await DbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to carry out scheduled product status update for multiple products. Transaction rolled back.");
                _logger.LogDebug($"Exception message: {exception.Message}");
                await transaction.RollbackAsync();
            }
        }

        public decimal CalculateDiscountedPrice(Product product)
        {
            if (product.Discount == null)
            {
                throw new InvalidOperationException($"Product ID {product.Id} does not have any discount");
            }

            return product.Price - product.Price * product.Discount.Percentage / 100;
        }
    }
}