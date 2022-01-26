using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.DTOs;
using Microsoft.EntityFrameworkCore;
using XWave.ViewModels.Management;
using XWave.Data.Constants;
using XWave.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : AbstractController<ProductController>
    {
        private readonly IProductService _productService;
        public ProductController(
            ILogger<ProductController> logger,
            IProductService productService
            ) : base(logger)
        {
            _productService = productService;
        }
        // GET: api/<ProductController>
        [HttpGet("customers/{categoryID:int?}")]
        public ActionResult<IEnumerable<ProductDTO>> GetForCustomers(int? categoryID)
        {
            return Ok(_productService.GetAllProductsForCustomers(categoryID));
        }

        [HttpGet("staff/{categoryID:int?}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<Product>>> GetForStaff(int? categoryID)
        {
            return Ok(await _productService.GetAllProductsForStaff(categoryID));
        }

        // GET api/<ProductController>/5
        [HttpGet("customers/{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            return Ok(await _productService.GetProductByIDForCustomers(id));
        }
        [HttpGet("staff/{id}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<Product>> GetByIDForStaff(int id)
        {
            return Ok(await _productService.GetAllProductsForStaff(id));
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProductAsync(productVM);
                if (result.Succeeded)
                {
                    return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/product/staff/{result.ResourceID}"));
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }

        // PUT api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductVM updatedProduct)
        {
            var product = await _productService.GetProductByIDForStaff(id);
            if (product == null)
            {
                return BadRequest(ResponseTemplate.NonExistentResource());
            }

            if (ModelState.IsValid)
            {
                var result = await _productService.UpdateProductAsync(id, updatedProduct);
                if (result.Succeeded)
                {
                    return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/product/staff/{result.ResourceID}"));
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }
        // DELETE api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var product = await _productService.GetProductByIDForStaff(id);
            if (product == null)
            {
                return BadRequest(ResponseTemplate.NonExistentResource());
            }
            var result = await _productService.DeleteProductAsync(id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Error);
        }

    }
}
