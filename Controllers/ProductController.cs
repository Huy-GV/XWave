﻿using Microsoft.AspNetCore.Authorization;
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
using XWave.DTOs.Customers;
using XWave.DTOs.Management;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkId=397860

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

        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetForCustomersAsync()
        {
            return Ok(await _productService.GetAllProductsForCustomers());
        }

        [HttpGet("staff")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<StaffProductDto>>> GetForStaff()
        {
            return Ok(await _productService.GetAllProductsForStaff());
        }

        // GET api/<ProductController>/5
        [HttpGet("customers/{id:int}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            return Ok(await _productService.GetProductByIdForCustomers(id));
        }
        [HttpGet("staff/{id:int}")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<StaffProductDto>> GetByIdForStaff(int id)
        {
            return Ok(await _productService.GetProductByIdForStaff(id));
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
        {

            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity); 
            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProductAsync(staffId, productViewModel);
                if (result.Succeeded)
                {
                    return XWaveCreated($"https://localhost:5001/api/product/staff/{result.ResourceId}");
                }

                return XWaveBadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }

        // PUT api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductViewModel updatedProduct)
        {
            var product = await _productService.GetProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (ModelState.IsValid)
            {
                var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
                var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);
                if (result.Succeeded)
                {
                    return XWaveCreated($"https://localhost:5001/api/product/staff/{result.ResourceId}");
                }

                return XWaveBadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }
        // DELETE api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var product = await _productService.GetProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }
            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _productService.DeleteProductAsync(staffId, id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }

    }
}
