using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Management;
using XWave.Web.Data;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiscountController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IDiscountService _discountService;

    public DiscountController(
        IDiscountService discountService,
        AuthenticationHelper authenticationHelper)
    {
        _discountService = discountService;
        _authenticationHelper = authenticationHelper;
    }

    [HttpGet]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult<IEnumerable<Discount>>> Get()
    {
        return Ok(await _discountService.FindAllDiscountsAsync());
    }

    [HttpGet("{id}/product")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<IEnumerable<Product>> GetProductsWithDiscount(int id)
    {
        //no need to include discount at this level
        return await _discountService.FindProductsWithDiscountIdAsync(id);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Discount>> GetAsync(int id)
    {
        var discount = await _discountService.FindDiscountByIdAsync(id);
        return discount is not null ? Ok(discount) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> CreateAsync([FromBody] DiscountViewModel newDiscount)
    {
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.CreateDiscountAsync(userId, newDiscount);

        return result.MapResult(
            this.Created($"{this.ApiUrl()}/discount/{result.Value}"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] DiscountViewModel updatedDiscount)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.UpdateDiscountAsync(managerId, id, updatedDiscount);

        return result.MapResult(this.Updated($"{this.ApiUrl()}/discount/{id}"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> Delete(int id)
    { 
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.RemoveDiscountAsync(managerId, id);
        return result.MapResult(NoContent());
    }

    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> ApplyDiscountToProduct(int id, [FromBody] int[] productIds)
    {
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _discountService.ApplyDiscountToProducts(userId, id, productIds);
        return result.MapResult(Ok());
    }

    [HttpPost("{id}/remove")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> RemoveDiscountFromProducts(int id, [FromBody] int[] productIds)
    {
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.RemoveDiscountFromProductsAsync(userId, id, productIds);
        return result.MapResult(Ok());
    }
}