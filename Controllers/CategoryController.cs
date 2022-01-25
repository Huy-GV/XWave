﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Data.Constants;
using XWave.Models;
using XWave.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : AbstractController<CategoryController>
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(
            ILogger<CategoryController> logger,
            ICategoryService categoryService) : base(logger)
        {
            _categoryService = categoryService;
        }
        // GET: api/<CategoryController>
        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get()
        {
            return Ok(await _categoryService.GetAllCategories());
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(int id)
        {
            return Ok(await _categoryService.GetCategoryByID(id));
        }

        // POST api/<CategoryController>
        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> CreateAsync([FromBody] Category newCategory)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.CreateCategory(newCategory);
                return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/category/admin/{result.ResourceID}"));
            }

            return BadRequest(ModelState);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] Category updatedCategory)
        {
            var category = await _categoryService.GetCategoryByID(id);
            if (category == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateCategory(id, updatedCategory);
                if (result.Succeeded)
                {
                    return Ok(ResponseTemplate
                    .Updated($"https://localhost:5001/api/category/admin/{result.ResourceID}"));
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByID(id);
            if (category == null)
            {
                return NotFound();
            }

            var result = await _categoryService.DeleteCategory(id);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Error);
        }
    }
}
