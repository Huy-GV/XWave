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
using XWave.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : AbstractController<ProductController>
    {
        private readonly IProductService _productService;
        private readonly AuthenticationHelper _authenticationHelper;
        public ProductController(
            ILogger<ProductController> logger,
            IProductService productService,
            AuthenticationHelper authenticationHelper) : base(logger)
        {
            _authenticationHelper = authenticationHelper;
            _productService = productService;
        }
        // GET: api/<ProductController>
        //TODO: add filter by a query
        //[HttpGet("customers/{categoryID:int?}")]
        //public ActionResult<IEnumerable<ProductDTO>> GetForCustomers(int? categoryID)
        //{
            
        //    return Ok(_productService.GetAllProductsForCustomers(categoryID));
        //}

        //[HttpGet("staff/{categoryID:int?}")]
        //[Authorize(Policy = "StaffOnly")]
        //public async Task<ActionResult<IEnumerable<Product>>> GetForStaff(int? categoryID)
        //{
        //    return Ok(await _productService.GetAllProductsForStaff(categoryID));
        //}

        [HttpGet("customers")]
        public ActionResult<IEnumerable<ProductDTO>> GetForCustomers()
        {

            return Ok(_productService.GetAllProductsForCustomers(null));
        }

        [HttpGet("staff")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<Product>>> GetForStaff()
        {
            return Ok(await _productService.GetAllProductsForStaff(null));
        }

        // GET api/<ProductController>/5
        [HttpGet("customers/{id:int}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            return Ok(await _productService.GetProductByIDForCustomers(id));
        }
        [HttpGet("staff/{id:int}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<Product>> GetByIDForStaff(int id)
        {
            return Ok(await _productService.GetProductByIDForStaff(id));
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
        {

            var staffID = _authenticationHelper.GetUserID(HttpContext.User.Identity); 
            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProductAsync(staffID, productViewModel);
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
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductViewModel updatedProduct)
        {
            var product = await _productService.GetProductByIDForStaff(id);
            if (product == null)
            {
                return BadRequest(ResponseTemplate.NonExistentResource());
            }

            if (ModelState.IsValid)
            {
                var staffID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
                var result = await _productService.UpdateProductAsync(staffID, id, updatedProduct);
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
            var staffID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            var result = await _productService.DeleteProductAsync(staffID, id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Error);
        }

    }
}
