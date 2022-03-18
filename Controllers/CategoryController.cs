using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkId=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : XWaveBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly AuthenticationHelper _authenticationHelper;
        public CategoryController(
            ICategoryService categoryService,
            AuthenticationHelper authenticationHelper)
        {
            _authenticationHelper = authenticationHelper;
            _categoryService = categoryService;
        }
        // GET: api/<CategoryController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> Get()
        {
            return Ok(await _categoryService.GetAllCategories());
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(int id)
        {
            return Ok(await _categoryService.GetCategoryById(id));
        }

        // POST api/<CategoryController>
        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> CreateAsync([FromBody] Category newCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _categoryService.CreateCategoryAsync(managerId, newCategory);
            return XWaveCreated($"https://localhost:5001/api/category/admin/{result.ResourceId}");
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _categoryService.UpdateCategoryAsync(managerId, id, updatedCategory);
            if (result.Succeeded)
            {
                return XWaveUpdated($"https://localhost:5001/api/category/admin/{result.ResourceId}");
            }

            return XWaveBadRequest(result.Error);
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _categoryService.DeleteCategoryAsync(managerId, id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }
    }
}
