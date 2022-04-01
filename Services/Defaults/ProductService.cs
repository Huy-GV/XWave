using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class ProductService : ServiceBase, IProductService
    {
        private readonly IActivityService _staffActivityService;
        private readonly ProductHelper _productHelper;

        public ProductService(
            XWaveDbContext dbContext,
            IActivityService staffActivityService,
            ProductHelper productHelper) : base(dbContext)
        {
            _productHelper = productHelper;
            _staffActivityService = staffActivityService;
        }

        public async Task<(ServiceResult, int? ProductId)> AddProductAsync(string staffId, ProductViewModel productViewModel)
        {
            try
            {
                if (!await DbContext.Category.AnyAsync(c => c.Id == productViewModel.CategoryId))
                {
                    return (ServiceResult.Failure("Category not found"), null);
                }

                var newProduct = new Product();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productViewModel);
                await DbContext.SaveChangesAsync();
                var result = await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Create);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Error);
                }

                return (ServiceResult.Success(), newProduct.Id);
            }
            catch (Exception ex)
            {
                return (ServiceResult.Failure(ex.Message), null);
            }
        }

        public async Task<ServiceResult> DeleteProductAsync(int productId)
        {
            try
            {
                var product = await DbContext.Product.FindAsync(productId);
                DbContext.Product.Update(product);
                product.IsDeleted = true;
                product.DeleteDate = DateTime.Now;
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
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
                .Select(p => _productHelper.CreateCustomerProductDTO(p));

            return Task.FromResult(productDtos);
        }

        public Task<IEnumerable<DetailedProductDto>> FindAllProductsForStaff(bool includeDiscontinuedProducts)
        {
            var products = DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                    .ThenInclude(d => d.Manager)
                .Include(p => p.Category)
                .AsEnumerable()
                .Where(p =>
                {
                    return includeDiscontinuedProducts || !p.IsDiscontinued;
                });

            return Task.FromResult(products
                .Select(p => _productHelper.CreateDetailedProductDto(p))
                .AsEnumerable());
        }

        public async Task<ProductDto?> FindProductByIdForCustomers(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : _productHelper.CreateCustomerProductDTO(product);
        }

        public async Task<DetailedProductDto?> FindProductByIdForStaff(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                    .ThenInclude(d => d.Manager)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : _productHelper.CreateDetailedProductDto(product);
        }

        public async Task<ServiceResult> UpdateProductAsync(
            string staffId,
            int id,
            ProductViewModel updatedProductViewModel)
        {
            try
            {
                var product = await DbContext.Product.FindAsync(id);
                if (product.IsDiscontinued || product.IsDeleted)
                {
                    return ServiceResult.Failure($"Product has been discontinued or removed.");
                }

                var entry = DbContext.Update(product);
                entry.CurrentValues.SetValues(updatedProductViewModel);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Modify);
                return ServiceResult.Success();
            }
            catch (Exception)
            {
                return ServiceResult.Failure("Failed to update product information.");
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
                product.Quantity = updatedStock;
                product.LatestRestock = DateTime.Now;
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Modify);
                return ServiceResult.Success();
            }
            catch (Exception)
            {
                return ServiceResult.Failure($"Failed to update stock of product with ID {productId} due to internal errors.");
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
                product.Price = updatedPrice;
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Modify);
                return ServiceResult.Success();
            }
            catch (Exception)
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} due to internal errors.");
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

            return ServiceResult.Success();
        }

        // todo: reuse this method?
        public async Task ScheduledUpdateProductPrice(string staffId, int productId, uint updatedPrice)
        {
            var product = await DbContext.Product.FindAsync(productId);
            if (product != null)
            {
                product.Price = updatedPrice;
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Modify);
            }
        }

        public async Task<ServiceResult> DiscontinueProductAsync(int productId, DateTime updateSchedule)
        {
            if (!await DbContext.Product.AnyAsync(p => p.Id == productId))
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} because it was not found.");
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
                methodCall: () => UpdateProductSaleStatusByScheduleAsync(productId, false, updateSchedule),
                delay: updateSchedule - now);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RestartProductSaleAsync(int productId, DateTime updateSchedule)
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
    }
}