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

        public async Task<ServiceResult> CreateProductAsync(string staffId, ProductViewModel productViewModel)
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
        public async Task<ServiceResult> DeleteProductAsync(string staffId, int productId)
        {
            try
            {
                DbContext.Product.Remove(await DbContext.Product.FindAsync(productId));
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Delete);
                return ServiceResult.Success();
            } 
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public Task<IEnumerable<ProductDto>> GetAllProductsForCustomers()
        {
            var productDtos = DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Select(p => _productHelper.CreateCustomerProductDTO(p))
                .AsEnumerable();

            return Task.FromResult(productDtos);
        }

        public Task<IEnumerable<DetailedProductDto>> GetAllProductsForStaff()
        {
            
            var products = DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                    .ThenInclude(d => d.Manager)
                .Include(p => p.Category)
                .Select(p => _productHelper.CreateDetailedProductDto(p))
                .AsEnumerable();
            
            return Task.FromResult(products);
        }

        public async Task<ProductDto> GetProductByIdForCustomers(int id)
        {
            var product = await DbContext.Product
                .AsNoTracking()
                .Include(p => p.Discount)
                .SingleOrDefaultAsync(p => p.Id == id);

            return _productHelper.CreateCustomerProductDTO(product);
        }

        public async Task<DetailedProductDto> GetProductByIdForStaff(int id)
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
                var entry = DbContext.Update(product);
                entry.CurrentValues.SetValues(updatedProductViewModel);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Product>(staffId, OperationType.Modify);
                return ServiceResult.Success(id.ToString());
            } 
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }
    }
}
