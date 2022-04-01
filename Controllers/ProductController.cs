using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Helpers;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
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

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllForCustomersAsync()
        {
            return Ok(await _productService.FindAllProductsForCustomers());
        }

        [HttpGet("private")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<IEnumerable<DetailedProductDto>>> GetAllForStaff()
        {
            return Ok(await _productService.FindAllProductsForStaff());
        }

        // GET api/<ProductController>/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var productDto = await _productService.FindProductByIdForCustomers(id);
            return productDto == null ? Ok(productDto) : NotFound();
        }

        [HttpGet("{id:int}/private")]
        public async Task<ActionResult<DetailedProductDto>> GetByIdForStaff(int id)
        {
            var productDto = await _productService.FindProductByIdForStaff(id);
            return productDto == null ? Ok(productDto) : NotFound();
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductViewModel productViewModel)
        {
            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var (result, productId) = await _productService.AddProductAsync(staffId, productViewModel);
            if (result.Succeeded)
            {
                return XWaveCreated($"https://localhost:5001/api/product/staff/{productId}");
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

            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _productService.UpdateProductAsync(staffId, id, updatedProduct);
            if (result.Succeeded)
            {
                return XWaveCreated($"https://localhost:5001/api/product/staff/{id}");
            }

            return XWaveBadRequest(result.Error);
        }

        //[Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}/price")]
        public async Task<ActionResult> UpdatePriceAsync(int id, [FromBody] ProductPriceAdjustmentViewModel viewModel)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            ServiceResult result;
            if (viewModel.Schedule == null)
            {
                result = await _productService.UpdateProductPriceAsync(staffId, id, viewModel.UpdatedPrice);
            }
            else
            {
                result = await _productService.UpdateProductPriceAsync(
                    staffId: staffId,
                    productId: id,
                    updatedPrice: viewModel.UpdatedPrice,
                    updateSchedule: viewModel.Schedule.Value);
            }

            if (result.Succeeded)
            {
                return XWaveUpdated($"https://localhost:5001/api/product/staff/{id}");
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

        //[Authorize(Roles = "manager")]
        [HttpPut("{id}/discontinue/{updateSchedule}")]
        public async Task<ActionResult> DiscontinueProductAsync(int id, DateTime updateSchedule)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (product.IsDiscontinued)
            {
                return BadRequest(XWaveResponse.Failed("Product is already discontinued"));
            }

            var result = await _productService.DiscontinueProductAsync(id, updateSchedule);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }

        //[Authorize(Roles = "manager")]
        [HttpPut("{id}/restart-sale/{updateSchedule}")]
        public async Task<ActionResult> RestartProductSaleAsync(int id, DateTime updateSchedule)
        {
            var product = await _productService.FindProductByIdForStaff(id);
            if (product == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (!product.IsDiscontinued)
            {
                return BadRequest(XWaveResponse.Failed("Product is currently in sale"));
            }

            var result = await _productService.RestartProductSaleAsync(id, updateSchedule);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }
    }
}