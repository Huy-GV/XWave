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
using XWave.ViewModels.Product;

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
            var products = DbContext.Product
                .Select(product => ProductDTO.From(product))
                .ToList();

            return Ok(products);
        }

        [HttpGet("staff")]
        [Authorize]
        public ActionResult<IEnumerable<Product>> GetAuthorized()
        {
            var products = DbContext.Product.ToList();
            return Ok(products);
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            var product = await DbContext.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(ProductDTO.From(product));
        }
        [HttpGet("staff/{id}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<Product>> GetAuthorized(int id)
        {
            var product = await DbContext.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST api/<ProductController>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult> Post([FromBody] ProductVM productVM)
        {
            if (!await ItemExists<Category>(productVM.CategoryID))
            {
                return BadRequest($"Category with ID {productVM.CategoryID} does not exist");
            }

            if (ModelState.IsValid)
            {
                Product newProduct = new();
                var entry = DbContext.Product.Add(newProduct);
                entry.CurrentValues.SetValues(productVM);
                await DbContext.SaveChangesAsync();
                return Ok( new
                {
                    Message = $"New product accessible via https://localhost:5001/api/product/admin/{newProduct.ID} for authorized users"
                });
            } 
            return BadRequest("Validation error");
        }


        // PUT api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] ProductVM updatedProduct)
        {
            var product = await DbContext.Product.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = "Product not found" });

            if (ModelState.IsValid)
            {
                var entry = DbContext.Attach(product);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(updatedProduct);
                await DbContext.SaveChangesAsync();
                return Ok(new
                {
                    Message = $"Updated product accessible via https://localhost:5001/api/product/admin/{product.ID} for authorized users"
                });
            }

            return BadRequest("An error occured");
        }
        // DELETE api/<ProductController>/5
        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!await ItemExists<Product>(id))
            {
                return NotFound(new { Message = "Product to be deleted not found" });
            }

            DbContext.Product.Remove(await DbContext.Product.FindAsync(id));
            DbContext.SaveChanges();
            return Ok(new { Message = $"Product with ID {id} deleted" });
        }

        private async Task<bool> ItemExists<T>(int id)
        {
            var entityTypeName = typeof(T).Name;
            switch (entityTypeName)
            { 
                case nameof(Product):
                    return await DbContext.Product.FindAsync(id) != null;
                case nameof(Category):
                    return await DbContext.Category.FindAsync(id) != null;
                case nameof(Discount):
                    return await DbContext.Discount.FindAsync(id) != null;
                default:
                    Logger.LogError($"Entity with type {entityTypeName} not found");
                    return false;
            }
        }
    }
}
