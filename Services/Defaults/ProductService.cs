using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class ProductService : ServiceBase, IProductService
    {
        private readonly IStaffActivityService _staffActivityService;
        private readonly ProductHelper _productHelper;
        public ProductService(
            XWaveDbContext dbContext,
            IStaffActivityService staffActivityService,
            ProductHelper productHelper) : base(dbContext) 
        { 
            _productHelper = productHelper;
            _staffActivityService = staffActivityService;
        }

        public async Task<ServiceResult> CreateProductAsync(string staffID, ProductViewModel productViewModel)
        {
            try
            {
                if (!await DbContext.Category.AnyAsync(c => c.ID == productViewModel.CategoryID))
                {
                    return ServiceResult.Failure("Category not found");
                }

                Product newProduct = new();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productViewModel);
                await DbContext.SaveChangesAsync();
                var result = await _staffActivityService.CreateLog<Product>(staffID, ActionType.Create);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Error);
                }

                return ServiceResult.Success(newProduct.ID.ToString());
            } catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }
        public async Task<ServiceResult> DeleteProductAsync(string staffID, int id)
        {
            try
            {
                DbContext.Product.Remove(await DbContext.Product.FindAsync(id));
                await DbContext.SaveChangesAsync();
                await _staffActivityService.CreateLog<Product>(staffID, ActionType.Delete);
                return ServiceResult.Success();
            } catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }

        }

        public Task<IEnumerable<ProductDTO>> GetAllProductsForCustomers()
        {
            var productDTOs = DbContext.Product
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Select(p => _productHelper.CreateCustomerProductDTO(p))
                .AsEnumerable();

            return Task.FromResult(productDTOs);
        }

        public Task<IEnumerable<StaffProductDTO>> GetAllProductsForStaff()
        {
            
            var products = DbContext.Product
                .Include(p => p.Discount)
                    .ThenInclude(d => d.Manager)
                .Include(p => p.Category)
                .Select(p => _productHelper.CreateStaffProductDTO(p))
                .AsEnumerable();
            
            return Task.FromResult(products);
        }

        public async Task<ProductDTO> GetProductByIDForCustomers(int id)
        {
            var product = await DbContext.Product
                .Include(p => p.Discount)
                .SingleOrDefaultAsync(p => p.ID == id);

            return _productHelper.CreateCustomerProductDTO(product);
        }

        public async Task<StaffProductDTO> GetProductByIDForStaff(int id)
        {
            var productDTO = await DbContext.Product
                        .Include(p => p.Discount)
                        .Include(p => p.Category)
                        .Select(p => _productHelper.CreateStaffProductDTO(p))
                        .FirstOrDefaultAsync(p => p.ID == id);

            return productDTO;
        }

        public async Task<ServiceResult> UpdateProductAsync(string staffID, int id, ProductViewModel updatedProduct)
        {
            try
            {
                var product = await DbContext.Product.FindAsync(id);
                var entry = DbContext.Attach(product);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(updatedProduct);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.CreateLog<Product>(staffID, ActionType.Modify);
                return ServiceResult.Success(id.ToString());
            } catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }
    }
}
