using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class ProductService : ServiceBase, IProductService
    {
        private readonly IStaffActivityService _staffActivityService;
        public ProductService(
            XWaveDbContext dbContext,
            IStaffActivityService staffActivityService) : base(dbContext) 
        { 
            _staffActivityService = staffActivityService;
        }

        public async Task<ServiceResult> CreateProductAsync(string staffID, ProductVM productVM)
        {
            try
            {
                if (!await DbContext.Category.AnyAsync(c => c.ID == productVM.CategoryID))
                {
                    return ServiceResult.Failure("Category not found");
                }

                Product newProduct = new();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productVM);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.CreateLog<Product>(staffID, ActionType.Create);
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

        public Task<IEnumerable<ProductDTO>> GetAllProductsForCustomers(int? categoryID = null)
        {

            var productDTOs = DbContext.Product
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Select(product => ProductDTO.From(product))
                .AsEnumerable();

            return Task.FromResult(categoryID == null ? 
                productDTOs : FilterByCategory(categoryID.Value, productDTOs));
        }

        public Task<IEnumerable<Product>> GetAllProductsForStaff(int? categoryID = null)
        {
            
            var products = DbContext.Product.AsEnumerable();
            var result = categoryID == null ? products : FilterByCategory(categoryID.Value, products);
            return Task.FromResult(result);
        }
        private static IEnumerable<T> FilterByCategory<T>(int categoryID, IEnumerable<T> source)
        {
            return source switch
            {
                IEnumerable<Product> products => products.Where(p => p.CategoryID == categoryID) as IEnumerable<T>,
                IEnumerable<ProductDTO> productDTOs => productDTOs.Where(p => p.CategoryID == categoryID) as IEnumerable<T>,
                _ => throw new ArgumentException("Generic must be Product or ProductDTO"),
            };
        }

        public async Task<ProductDTO> GetProductByIDForCustomers(int id)
        {
            var products = await DbContext.Product
                .Include(p => p.Discount)
                .SingleOrDefaultAsync(p => p.ID == id);

            return ProductDTO.From(products);
        }

        public async Task<Product> GetProductByIDForStaff(int id)
        {
            return await DbContext.Product
                    .Include(p => p.Discount)
                    .SingleOrDefaultAsync(p => p.ID == id);
        }

        public async Task<ServiceResult> UpdateProductAsync(string staffID, int id, ProductVM updatedProduct)
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
