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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : AbstractController<ProductController>
    {
        public ProductController(
            XWaveDbContext dbContext,
            ILogger<ProductController> logger
            ) : base(dbContext, logger)
        {

        }
        // GET: api/<ProductController>
        [HttpGet]
        public ActionResult<IEnumerable<ProductDTO>> Get()
        {
            //TODO: include category
            var products = DbContext.Product
                .Include(p => p.Discount)
                .Include(p => p.Category)
                .Select(product => ProductDTO.From(product))
                .ToList();

            return Ok(products);
        }

        [HttpGet("full-details/{categoryID:int?}")]
        [Authorize]
        public ActionResult<IEnumerable<Product>> GetFullDetails(int? categoryID)
        {
            var products = DbContext.Product.AsEnumerable();

            if (categoryID != null)
                products = products.Where(p => p.CategoryID == categoryID);

            return Ok(products);
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            var product = await DbContext.Product
                .Include(p => p.Discount)
                .SingleOrDefaultAsync(p => p.ID == id);

            if (product == null)
                return NotFound();

            return Ok(ProductDTO.From(product));
        }
        [HttpGet("{id}/full-details")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<Product>> GetFullDetails(int id)
        {
            var product = await DbContext.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> CreateAsync([FromBody] ProductVM productVM)
        {
            if (!await ItemExistsAsync<Category>(productVM.CategoryID))
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                Product newProduct = new();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productVM);
                await DbContext.SaveChangesAsync();

                return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/product/admin/{newProduct.ID}"));
            }

            return BadRequest(ModelState);
        }


        // PUT api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProductVM updatedProduct)
        {
            var product = await DbContext.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var entry = DbContext.Attach(product);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(updatedProduct);
                await DbContext.SaveChangesAsync();
                return Ok(ResponseTemplate
                    .Updated($"https://localhost:5001/api/product/admin/{product.ID}"));
            }

            return BadRequest(ModelState);
        }
        // DELETE api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            if (!await ItemExistsAsync<Product>(id))
                return NotFound();

            DbContext.Product.Remove(await DbContext.Product.FindAsync(id));
            DbContext.SaveChanges();
            return Ok(ResponseTemplate.Deleted(id.ToString(), nameof(Product)));
        }

    }
}
