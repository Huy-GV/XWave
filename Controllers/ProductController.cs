using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Data.Constants;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Extensions;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.Utils;
using XWave.ViewModels.Management;

namespace XWave.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IProductService _productService;

    public ProductController(
        IProductService productService,
        AuthenticationHelper authenticationHelper,
        IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
        _authenticationHelper = authenticationHelper;
        _productService = productService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllForCustomersAsync()
    {
        return Ok(await _productService.FindAllProductsForCustomers());
    }

    [HttpGet("private")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult<IEnumerable<DetailedProductDto>>> GetAllForStaff()
    {
        return Ok(await _productService.FindAllProductsForStaff());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var productDto = await _productService.FindProductByIdForCustomers(id);
        return productDto != null ? Ok(productDto) : NotFound();
    }

    [HttpGet("{id:int}/private")]
    public async Task<ActionResult<DetailedProductDto>> GetByIdForStaff(int id)
    {
        var productDto = await _productService.FindProductByIdForStaff(id);
        return productDto != null ? Ok(productDto) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var (result, productId) = await _productService.AddProductAsync(staffId, productViewModel);
        return result.Succeeded
            ? this.XWaveCreated($"https://localhost:5001/api/product/staff/{productId}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductViewModel updatedProduct)
    {
        var product = await _productService.FindProductByIdForStaff(id);
        if (product == null) return BadRequest(XWaveResponse.NonExistentResource());

        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);
        return result.Succeeded
            ? this.XWaveCreated($"https://localhost:5001/api/product/staff/{id}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdatePriceAsync(int id, [FromBody] ProductPriceAdjustmentViewModel viewModel)
    {
        var product = await _productService.FindProductByIdForStaff(id);
        if (product == null) return BadRequest(XWaveResponse.NonExistentResource());

        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        ServiceResult result;
        if (viewModel.Schedule == null)
            result = await _productService.UpdateProductPriceAsync(staffId, id, viewModel.UpdatedPrice);
        else
            result = await _productService.UpdateProductPriceAsync(
                staffId,
                id,
                viewModel.UpdatedPrice,
                viewModel.Schedule.Value);

        return result.Succeeded
            ? this.XWaveUpdated($"https://localhost:5001/api/product/staff/{id}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var product = await _productService.FindProductByIdForStaff(id);
        if (product == null) return NotFound(XWaveResponse.NonExistentResource());

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DeleteProductAsync(id, managerId);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("discontinue/{updateSchedule}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> DiscontinueProductAsync([FromBody] int[] ids, DateTime updateSchedule)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DiscontinueProductAsync(managerId, ids, updateSchedule);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("{id:int}/restart-sale/{updateSchedule:datetime}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> RestartProductSaleAsync(int id, DateTime updateSchedule)
    {
        var product = await _productService.FindProductByIdForStaff(id);
        if (product == null) return NotFound(XWaveResponse.NonExistentResource());

        if (!product.IsDiscontinued) return BadRequest(XWaveResponse.Failed("Product is currently in sale."));

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.RestartProductSaleAsync(managerId, id, updateSchedule);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Errors);
    }

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> CancelBackgroundTaskAsync(string id)
    {
        var result = await _backgroundJobService.CancelJobAsync(id);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Errors);
    }
}