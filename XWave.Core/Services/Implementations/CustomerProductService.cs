using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Implementations;

internal class CustomerProductService : ServiceBase, ICustomerProductService
{
    private readonly IActivityService _activityService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<ProductManagementService> _logger;
    private readonly ProductDtoMapper _productDtoMapper;
    private readonly IAuthorizationService _authorizationService;

    private readonly string[] staffRoles = new[] { RoleNames.Staff, RoleNames.Manager };

    private readonly Error _unauthorizedError = new()
    {
        Code = ErrorCode.AuthorizationError,
        Message = "Only staff are authorized to modify products"
    };

    public CustomerProductService(
        XWaveDbContext dbContext,
        IActivityService activityService,
        IBackgroundJobService backgroundJobService,
        ProductDtoMapper productHelper,
        ILogger<ProductManagementService> logger,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _productDtoMapper = productHelper;
        _activityService = activityService;
        _authorizationService = authorizationService;
        _backgroundJobService = backgroundJobService;
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
                : _productDtoMapper.MapCustomerProductDto(p, CalculatePriceAfterDiscount(p)))
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

        if (product is null) return null;

        return product.Discount is null
            ? _productDtoMapper.MapCustomerProductDto(product)
            : _productDtoMapper.MapCustomerProductDto(product, CalculatePriceAfterDiscount(product));
    }

    public decimal CalculatePriceAfterDiscount(Product product)
    {
        if (product.Discount is null) 
        {
            throw new InvalidOperationException($"Product ID {product.Id} does not have any discount");
        }
        
        return product.Price - product.Price * product.Discount.Percentage / 100;
    }
}