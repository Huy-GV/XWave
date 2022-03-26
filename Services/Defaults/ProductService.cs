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

        public async Task<ServiceResult> AddProductAsync(string staffId, ProductViewModel productViewModel)
        {
            try
            {
                if (!await DbContext.Category.AnyAsync(c => c.Id == productViewModel.CategoryId))
                {
                    return ServiceResult.Failure("Category not found");
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

                return ServiceResult.Success(newProduct.Id.ToString());
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
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

        public async Task<ServiceResult> UpdateDiscontinuationStatus(int id, bool isDiscontinued)
        {
            try
            {
                var product = await DbContext.Product.FindAsync(id);
                DbContext.Product.Update(product);
                product.IsDiscontinued = isDiscontinued;
                product.DiscontinuationDate = isDiscontinued ? DateTime.Now : null;
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
                .Select(p => _productHelper.CreateCustomerProductDTO(p))
                .AsEnumerable();

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

        public async Task<ProductDto> FindProductByIdForCustomers(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .SingleOrDefaultAsync(p => p.Id == id);

            return _productHelper.CreateCustomerProductDTO(product);
        }

        public async Task<DetailedProductDto> FindProductByIdForStaff(int id)
        {
            var productDto = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Select(p => _productHelper.CreateDetailedProductDto(p))
                .FirstOrDefaultAsync(p => p.Id == id);

            return productDto;
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
                return ServiceResult.Success(id.ToString());
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
                return ServiceResult.Success(productId.ToString());
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
                return ServiceResult.Success(productId.ToString());
            }
            catch (Exception)
            {
                return ServiceResult.Failure($"Failed to update price of product with ID {productId} due to internal errors.");
            }
        }

        public Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice, DateTime updateSchedule)
        {
            // todo: use hangfire to schedule price update
            throw new NotImplementedException();
        }

        public Task<ServiceResult> UpdateDiscontinuationStatus(int id, bool isDiscontinued, DateTime updateSchedule)
        {
            throw new NotImplementedException();
        }
    }
}