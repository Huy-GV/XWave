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
    private readonly IProductManagementService _productService;
    private readonly ICustomerProductService _customerProductService;

    public ProductController(
        IProductManagementService productService,
        AuthenticationHelper authenticationHelper,
        IBackgroundJobService backgroundJobService,
        ICustomerProductService customerProductService)
    {
        _backgroundJobService = backgroundJobService;
        _authenticationHelper = authenticationHelper;
        _productService = productService;
        _customerProductService = customerProductService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllForCustomersAsync()
    {
        return Ok(await _customerProductService.FindAllProducts());
    }

    [HttpGet("private")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult<IEnumerable<DetailedProductDto>>> GetAllForStaff()
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.FindAllProductsForStaff(false, staffId);

        return result.OnSuccess(x => Ok(x));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var productDto = await _customerProductService.FindProduct(id);
        return productDto is not null ? Ok(productDto) : NotFound();
    }

    [HttpGet("{id:int}/private")]
    public async Task<ActionResult<DetailedProductDto>> GetByIdForStaff(int id)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.FindProductByIdForStaff(id, staffId);

        return result.OnSuccess(x => Ok(x));
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> CreateAsync([FromBody] CreateProductViewModel productViewModel)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.AddProductAsync(staffId, productViewModel);

        return result.OnSuccess(x => this.Created($"{this.ApiUrl()}/product/{x}/private"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] UpdateProductViewModel updatedProduct)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);

        return result.OnSuccess(() => this.Created($"{this.ApiUrl()}/product/{id}/private"));
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> UpdatePriceAsync(int id, [FromBody] UpdateProductPriceViewModel viewModel)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.UpdateProductPriceAsync(staffId, id, viewModel);

        return result.OnSuccess(() => this.Created($"{this.ApiUrl()}/product/{id}/private"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DeleteProductAsync(id, managerId);
        return result.OnSuccess(() => NoContent());
    }

    [HttpPut("discontinue/{updateSchedule}")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> DiscontinueProductAsync([FromBody] int[] ids, DateTime updateSchedule)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.DiscontinueProductAsync(managerId, ids, updateSchedule);
        return result.OnSuccess(() => NoContent());
    }

    [HttpPut("{id:int}/restart-sale/{updateSchedule:datetime}")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> RestartProductSaleAsync(int id, DateTime updateSchedule)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _productService.RestartProductSaleAsync(managerId, id, updateSchedule);
        return result.OnSuccess(() => NoContent());
    }

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> CancelBackgroundTaskAsync(string id)
    {
        var result = await _backgroundJobService.CancelJobAsync(id);
        return result.OnSuccess(() => NoContent());
    }
}