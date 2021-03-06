using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Web.Data;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly ICategoryService _categoryService;

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

    [HttpGet("{id:int}")]
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
        var result = await _categoryService.AddCategoryAsync(managerId, newCategory);
        return result.Succeeded
            ? this.XWaveCreated($"https://localhost:5001/api/category/admin/{result.Value}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
    {
        var category = await _categoryService.FindCategoryByIdAsync(id);
        if (category == null) return NotFound(XWaveResponse.NonExistentResource());

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.UpdateCategoryAsync(managerId, id, updatedCategory);
        return result.Succeeded
            ? this.XWaveUpdated($"https://localhost:5001/api/category/admin/{id}")
            : UnprocessableEntity(result.Errors);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Manager))]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _categoryService.FindCategoryByIdAsync(id);
        if (category == null) return NotFound(XWaveResponse.NonExistentResource());

        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.DeleteCategoryAsync(managerId, id);
        if (result.Succeeded) return NoContent();

        return UnprocessableEntity(result.Errors);
    }
}