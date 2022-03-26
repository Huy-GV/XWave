using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Helpers;
using XWave.Services.Interfaces;
using XWave.ViewModels.Management;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkId=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : XWaveBaseController
    {
        private readonly IProductService _productService;
        private readonly AuthenticationHelper _authenticationHelper;

        public ProductController(
            IProductService productService,
            AuthenticationHelper authenticationHelper)
        {
            _authenticationHelper = authenticationHelper;
            _productService = productService;
        }

        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetForCustomersAsync()
        {
            return Ok(await _productService.FindAllProductsForCustomers());
        }

        [HttpGet("staff")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<DetailedProductDto>>> GetForStaff()
        {
            return Ok(await _productService.FindAllProductsForStaff());
        }

        // GET api/<ProductController>/5
        [HttpGet("customers/{id:int}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            return Ok(await _productService.FindProductByIdForCustomers(id));
        }

        [HttpGet("staff/{id:int}")]
        public async Task<ActionResult<DetailedProductDto>> GetByIdForStaff(int id)
        {
            return Ok(await _productService.FindProductByIdForStaff(id));
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
        {
            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.AddProductAsync(staffId, productViewModel);
            if (result.Succeeded)
            {
                return XWaveCreated($"https://localhost:5001/api/product/staff/{result.ResourceId}");
            }

            return XWaveBadRequest(result.Error);
        }

        // PUT api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductViewModel updatedProduct)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);
            if (result.Succeeded)
            {
                return XWaveCreated($"https://localhost:5001/api/product/staff/{result.ResourceId}");
            }

            return XWaveBadRequest(result.Error);
        }

        // DELETE api/<ProductController>/5
        [Authorize(Roles = "manager")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _productService.DeleteProductAsync(id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }

        [Authorize(Roles = "manager")]
        [HttpPost("discontinue/{id}/{isDiscontinued:bool}")]
        public async Task<ActionResult> UpdateStatusAsync(int id, bool isDiscontinued)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _productService.UpdateDiscontinuationStatus(id, isDiscontinued);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }
    }
}