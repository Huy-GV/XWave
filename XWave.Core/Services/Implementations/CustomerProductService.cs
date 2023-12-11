using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.Extension;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;

namespace XWave.Core.Services.Implementations;

internal class CustomerProductService : ServiceBase, ICustomerProductService
{
    private readonly IActivityService _activityService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<ProductManagementService> _logger;
    private readonly ProductDtoMapper _productDtoMapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDiscountedProductPriceCalculator _discountedPriceCalculator;

    public CustomerProductService(
        XWaveDbContext dbContext,
        IActivityService activityService,
        IBackgroundJobService backgroundJobService,
        ProductDtoMapper productHelper,
        ILogger<ProductManagementService> logger,
        IAuthorizationService authorizationService,
        IDiscountedProductPriceCalculator discountedPriceCalculator) : base(dbContext)
    {
        _productDtoMapper = productHelper;
        _activityService = activityService;
        _authorizationService = authorizationService;
        _backgroundJobService = backgroundJobService;
        _discountedPriceCalculator = discountedPriceCalculator;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<ProductDto>> FindAllProducts()
    {
        var productDtos = DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .Where(p => !p.IsDiscontinued)
            .AsEnumerable()
            .Select(p => p.Discount is null
                ? _productDtoMapper.MapCustomerProductDto(p)
                : _productDtoMapper.MapCustomerProductDto(p, _discountedPriceCalculator.CalculatePriceAfterDiscount(p)))
            .ToList();

        return Task.FromResult(productDtos.AsIReadonlyCollection());
    }

    public async Task<ProductDto?> FindProduct(int id)
    {
        var product = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
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