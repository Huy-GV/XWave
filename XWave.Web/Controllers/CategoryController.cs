using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Web.Utils;
using XWave.Web.Data;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
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
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.FindAllCategoriesAsync(userId);

        return result.OnSuccess(() => Ok(result.Value));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> Get(int id)
    {
        var userId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.FindCategoryByIdAsync(id, userId);

        return result.OnSuccess(() => Ok(result.Value));
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> CreateAsync([FromBody] Category newCategory)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.AddCategoryAsync(managerId, newCategory);

        return result.OnSuccess(x => this.Created($"{this.ApiUrl()}/category/admin/{result.Value}"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _categoryService.UpdateCategoryAsync(managerId, id, updatedCategory);

        return result.OnSuccess(() => this.Updated($"{this.ApiUrl()}/category/admin/{id}"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(RoleNames.Manager))]
    public async Task<ActionResult> Delete(int id)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (string.IsNullOrEmpty(managerId))
        {
            return Unauthorized();
        }

        var result = await _categoryService.DeleteCategoryAsync(managerId, id);

        return result.OnSuccess(() => NoContent());
    }
}