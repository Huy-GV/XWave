using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Extensions;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.ViewModels.Management;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly AuthenticationHelper _authenticationHelper;

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
            var (result, productId) = await _discountService.CreateDiscountAsync(userId, newDiscount);
            if (result.Succeeded)
            {
                return this.XWaveCreated($"https://localhost:5001/api/discount/{productId}");
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] DiscountViewModel updatedDiscount)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _discountService.UpdateDiscountAsync(managerId, id, updatedDiscount);
            if (result.Succeeded)
            {
                return this.XWaveUpdated($"https://localhost:5001/api/discount/{id}");
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> Delete(int id)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _discountService.RemoveDiscountAsync(managerId, id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> ApplyDiscountToProduct(int id, [FromBody] int[] productIds)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _discountService.ApplyDiscountToProducts(userId, id, productIds);
            if (result.Succeeded)
            {
                return Ok("Discount has been successfully applied to selected products.");
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpPost("{id}/remove")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> RemoveDiscountFromProducts(int id, [FromBody] int[] productIds)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _discountService.RemoveDiscountFromProductsAsync(userId, id, productIds);
            if (result.Succeeded)
            {
                return Ok("Discount has been successfully applied to selected products.");
            }

            return this.XWaveBadRequest(result.Errors);
        }
    }
}