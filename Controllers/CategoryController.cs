using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Extensions;
using XWave.Utils;
using XWave.Models;
using XWave.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkId=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> Get()
        {
            return Ok(await _categoryService.FindAllCategoriesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(int id)
        {
            var category = await _categoryService.FindCategoryByIdAsync(id);
            return category != null ? Ok(category) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> CreateAsync([FromBody] Category newCategory)
        {
            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var (result, categoryId) = await _categoryService.AddCategoryAsync(managerId, newCategory);
            if (result.Succeeded)
            {
                return this.XWaveCreated($"https://localhost:5001/api/category/admin/{categoryId}");
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
        {
            var category = await _categoryService.FindCategoryByIdAsync(id);
            if (category == null)
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _categoryService.UpdateCategoryAsync(managerId, id, updatedCategory);
            if (result.Succeeded)
            {
                return this.XWaveUpdated($"https://localhost:5001/api/category/admin/{id}");
            }

            return this.XWaveBadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(Roles.Manager))]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _categoryService.FindCategoryByIdAsync(id);
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

            return this.XWaveBadRequest(result.Errors);
        }
    }
}