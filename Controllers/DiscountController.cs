using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.ViewModels.Management;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : XWaveBaseController
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

        // GET: api/<DiscountController>
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<Discount>>> Get()
        {
            return Ok(await _discountService.FindAllDiscountsAsync());
        }

        [HttpGet("{id}/product")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IEnumerable<Product>> GetProductsWithDiscount(int id)
        {
            //no need to include discount at this level
            return await _discountService.FindProductsWithDiscountIdAsync(id);
        }

        // GET api/<DiscountController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetAsync(int id)
        {
            return Ok(await _discountService.FindDiscountByIdAsync(id));
        }

        // POST api/<DiscountController>
        [HttpPost]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> CreateAsync([FromBody] DiscountViewModel newDiscount)
        {
            var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _discountService.CreateDiscountAsync(userId, newDiscount);
            if (result.Succeeded)
            {
                return XWaveCreated($"https://localhost:5001/api/discount/{result.ResourceId}");
            }

            return XWaveBadRequest(result.Error);
        }

        // PUT api/<DiscountController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] DiscountViewModel updatedDiscount)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _discountService.UpdateDiscountAsync(managerId, id, updatedDiscount);
            if (result.Succeeded)
            {
                return XWaveUpdated($"https://localhost:5001/api/discount/{result.ResourceId}");
            }

            return XWaveBadRequest(result.Error);
        }

        // DELETE api/<DiscountController>/5
        [HttpDelete("{id}")]
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

            return XWaveBadRequest(result.Error);
        }

        [HttpPost("{id}/apply")]
        public async Task<ActionResult> ApplyDiscountToProduct(int id, [FromBody] int[] productIds)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _discountService.ApplyDiscountToProducts(id, productIds);
            if (result.Succeeded)
            {
                return Ok("Discount has been successfully applied to selected products.");
            }

            return XWaveBadRequest(result.Error);
        }

        [HttpPost("{id}/remove")]
        public async Task<ActionResult> RemoveDiscountFromProducts(int id, [FromBody] int[] productIds)
        {
            if (await _discountService.FindDiscountByIdAsync(id) == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _discountService.RemoveDiscountFromProductsAsync(id, productIds);
            if (result.Succeeded)
            {
                return Ok("Discount has been successfully applied to selected products.");
            }

            return XWaveBadRequest(result.Error);
        }
    }
}