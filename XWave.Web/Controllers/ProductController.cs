using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Management;
using XWave.Web.Data;
using XWave.Web.Extensions;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

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
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.FindAllProductsForStaff(false, staffId);
    
        return result.Succeeded 
            ? Ok(result.Value)
            : UnprocessableEntity();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var productDto = await _productService.FindProductByIdForCustomers(id);
        return productDto is not null ? Ok(productDto) : NotFound();
    }

    [HttpGet("{id:int}/private")]
    public async Task<ActionResult<DetailedProductDto>> GetByIdForStaff(int id)
    {
        var productDto = await _productService.FindProductByIdForStaff(id);
        return productDto is not null ? Ok(productDto) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.AddProductAsync(staffId, productViewModel);
        return result.Succeeded
            ? this.Created($"https://localhost:5001/api/product/staff/{result.Value}")
            : UnprocessableEntity(result.Error);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductViewModel updatedProduct)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);
        return result.Succeeded
            ? this.Created($"https://localhost:5001/api/product/staff/{id}")
            : UnprocessableEntity(result.Error);
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdatePriceAsync(int id, [FromBody] ProductPriceAdjustmentViewModel viewModel)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = viewModel.Schedule is null
            ? await _productService.UpdateProductPriceAsync(staffId, id, viewModel.UpdatedPrice)
            : await _productService.UpdateProductPriceAsync(
                staffId,
                id,
                viewModel.UpdatedPrice,
                viewModel.Schedule.Value);

        return result.Succeeded
            ? this.Updated($"https://localhost:5001/api/product/staff/{id}")
            : UnprocessableEntity(result.Error);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DeleteProductAsync(id, managerId);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Error);
    }

    [HttpPut("discontinue/{updateSchedule}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> DiscontinueProductAsync([FromBody] int[] ids, DateTime updateSchedule)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DiscontinueProductAsync(managerId, ids, updateSchedule);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Error);
    }

    [HttpPut("{id:int}/restart-sale/{updateSchedule:datetime}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> RestartProductSaleAsync(int id, DateTime updateSchedule)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.RestartProductSaleAsync(managerId, id, updateSchedule);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Error);
    }

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> CancelBackgroundTaskAsync(string id)
    {
        var result = await _backgroundJobService.CancelJobAsync(id);
        return result.Succeeded
            ? NoContent()
            : UnprocessableEntity(result.Error);
    }
}