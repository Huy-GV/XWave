using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using XWave.Data;
using XWave.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XWave.Data.Constants;
using XWave.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using XWave.ViewModels.Management;
using XWave.Services ;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : AbstractController<DiscountController>
    {
        private readonly IDiscountService _discountService;
        private readonly IAuthenticationService _authenticationService;
        public DiscountController(
            XWaveDbContext dbContext,
            ILogger<DiscountController> logger,
            IDiscountService discountService,
            IAuthenticationService authService
        ) : base(logger)
        {
            _discountService = discountService;
            _authenticationService = authService;
        }
        // GET: api/<DiscountController>
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<Discount>>> Get()
        {
            return Ok(await _discountService.GetAllAsync());
        }
        [HttpGet("{id}/product")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IEnumerable<Product>> GetProductsWithDiscount(int id)
        {
            //no need to include discount at this level
            return await _discountService.GetProductsByDiscountID(id);
        }

        // GET api/<DiscountController>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<Discount>> GetAsync(int id)
        {
            return Ok(_discountService.GetAsync(id));
        }

        // POST api/<DiscountController>
        [HttpPost]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> CreateAsync([FromBody] DiscountVM newDiscount)
        {
            //TODO: move the following to auth service
            var userID = _authenticationService.GetUserID(HttpContext.User.Identity);
            if (ModelState.IsValid)
            {
                var result = await _discountService.CreateAsync(userID, newDiscount);
                if (result.Succeeded)
                {
                    return Ok(ResponseTemplate.Created($"https://localhost:5001/api/discount/{result.ResourceID}"));
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }

        // PUT api/<DiscountController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] DiscountVM updatedDiscount)
        {
            if (await _discountService.GetAsync(id) == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _discountService.UpdateAsync(id, updatedDiscount);
                if (result.Succeeded)
                {
                    return Ok(ResponseTemplate.Updated($"https://localhost:5001/api/discount/{result.ResourceID}"));
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }

        // DELETE api/<DiscountController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _discountService.DeleteAsync(id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Error);
        }
    }
}
