using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
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
        return discount != null ? Ok(discount) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> CreateAsync([FromBody] DiscountViewModel newDiscount)
    {
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.CreateDiscountAsync(userId, newDiscount);
        return result.Succeeded
            ? this.XWaveCreated($"https://localhost:5001/api/discount/{result.Value}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] DiscountViewModel updatedDiscount)
    {
        if (await _discountService.FindDiscountByIdAsync(id) == null)
            return NotFound(XWaveResponse.NonExistentResource());

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.UpdateDiscountAsync(managerId, id, updatedDiscount);
        return result.Succeeded
            ? this.XWaveUpdated($"https://localhost:5001/api/discount/{id}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> Delete(int id)
    {
        if (await _discountService.FindDiscountByIdAsync(id) == null)
            return NotFound(XWaveResponse.NonExistentResource());

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.RemoveDiscountAsync(managerId, id);
        if (result.Succeeded) return NoContent();

        return UnprocessableEntity(result.Errors);
    }

    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> ApplyDiscountToProduct(int id, [FromBody] int[] productIds)
    {
        if (await _discountService.FindDiscountByIdAsync(id) == null)
            return NotFound(XWaveResponse.NonExistentResource());

        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.ApplyDiscountToProducts(userId, id, productIds);
        if (result.Succeeded) return Ok("Discount has been successfully applied to selected products.");

        return UnprocessableEntity(result.Errors);
    }

    [HttpPost("{id}/remove")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> RemoveDiscountFromProducts(int id, [FromBody] int[] productIds)
    {
        if (await _discountService.FindDiscountByIdAsync(id) == null)
            return NotFound(XWaveResponse.NonExistentResource());

        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _discountService.RemoveDiscountFromProductsAsync(userId, id, productIds);
        if (result.Succeeded) return Ok("Discount has been successfully removed from selected products.");

        return UnprocessableEntity(result.Errors);
    }
}