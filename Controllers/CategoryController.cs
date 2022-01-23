using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Data.Constants;
using XWave.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : AbstractController<CategoryController>
    {
        public CategoryController(
            ILogger<CategoryController> logger,
            XWaveDbContext dbContext) : base(dbContext, logger)
        {
            
        }
        // GET: api/<CategoryController>
        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get()
        {
            return Ok(DbContext.Category.ToList());
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public ActionResult<Category> Get(int id)
        {
            var category = DbContext.Category.FirstOrDefault(category => category.ID == id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST api/<CategoryController>
        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> CreateAsync([FromBody] Category newCategory)
        {
            if (ModelState.IsValid)
            {
                DbContext.Category.Add(newCategory);
                await DbContext.SaveChangesAsync();
                return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/category/admin/{newCategory.ID}"));
            }

            return BadRequest(ModelState);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
        {
            var category = await DbContext.Category.FindAsync(id);
            if (category == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                DbContext.Category.Update(updatedCategory);
                return Ok(ResponseTemplate
                    .Updated($"https://localhost:5001/api/category/admin/{updatedCategory.ID}"));
            }

            return BadRequest(ModelState);
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await DbContext.Category.FindAsync(id);
            if (category == null)
                return NotFound();

            try
            {
                DbContext.Category.Remove(category);
                await DbContext.SaveChangesAsync();
            } catch (Exception e)
            {
                Logger.LogError($"Error deleting a category: {e}");
                return BadRequest("Foreign key constraint failed");
            }

            return NoContent();
        }
    }
}
