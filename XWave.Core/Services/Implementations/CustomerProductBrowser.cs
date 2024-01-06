using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.DTOs.Customers;
using XWave.Core.Extension;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;

namespace XWave.Core.Services.Implementations;

internal class CustomerProductBrowser : ServiceBase, ICustomerProductBrowser
{
    private readonly ProductDtoMapper _productDtoMapper;
    private readonly IDiscountedProductPriceCalculator _discountedPriceCalculator;

    public CustomerProductBrowser(
        XWaveDbContext dbContext,
        ProductDtoMapper productDtoMapper,
        IDiscountedProductPriceCalculator discountedPriceCalculator) : base(dbContext)
    {
        _productDtoMapper = productDtoMapper;
        _discountedPriceCalculator = discountedPriceCalculator;
    }

    public async Task<IReadOnlyCollection<ProductDto>> FindAllProducts()
    {
        var productDtos = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .Where(p => !p.IsDiscontinued && !p.IsDeleted)
            .ToListAsync();

        return productDtos
            .Select(p => p.Discount is null
                ? _productDtoMapper.MapCustomerProductDto(p)
                : _productDtoMapper.MapCustomerProductDto(p, _discountedPriceCalculator.CalculatePriceAfterDiscount(p)))
            .ToList()
            .AsIReadonlyCollection();
    }


    public async Task<ProductDto?> FindProduct(int id)
    {
        var product = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .Where(p => !p.IsDiscontinued && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return null;
        }

        return product.Discount is null
            ? _productDtoMapper.MapCustomerProductDto(product)
            : _productDtoMapper.MapCustomerProductDto(product, _discountedPriceCalculator.CalculatePriceAfterDiscount(product));
    }
}
