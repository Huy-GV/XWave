﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using XWave.Data;
using XWave.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XWave.Data.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : AbstractController<DiscountController>
    {
        public DiscountController(
            XWaveDbContext dbContext,
            ILogger<DiscountController> logger
        ) : base(dbContext, logger)
        {

        }
        // GET: api/<DiscountController>
        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public IEnumerable<Discount> Get()
        {
            return DbContext.Discount.ToList(); ;
        }
        [HttpGet("{id}/product")]
        //[Authorize(Policy = "StaffOnly")]
        public async Task<IEnumerable<Product>> GetProductsWithDiscount(int id)
        {
            //no need to include discount at this level
            return await DbContext.Product
                .Where(p => p.DiscountID == id)
                .ToListAsync(); ;
        }

        // GET api/<DiscountController>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<Discount>> GetAsync(int id)
        {
            var discount = await DbContext.Discount.SingleOrDefaultAsync(d => d.ID == id);
            if (discount == null)
                return NotFound();

            return Ok(discount);
        }

        // POST api/<DiscountController>
        [HttpPost]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> CreateAsync([FromBody] Discount newDiscount)
        {
            if (ModelState.IsValid)
            {
                DbContext.Discount.Add(newDiscount);
                await DbContext.SaveChangesAsync();
                return Ok(ResponseTemplate.Created($"https://localhost:5001/api/discount/{newDiscount.ID}"));
            }

            return BadRequest(ModelState);
        }

        // PUT api/<DiscountController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "managers")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] Discount updatedDiscount)
        {
            if (await DbContext.Discount.FirstOrDefaultAsync(d => d.ID == id) == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                DbContext.Discount.Update(updatedDiscount);
                await DbContext.SaveChangesAsync();
                return Ok(ResponseTemplate
                    .Updated($"https://localhost:5001/api/discount/{updatedDiscount.ID}"));
            }

            return BadRequest(ModelState);
        }

        // DELETE api/<DiscountController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="manager")]
        public async Task<ActionResult> Delete(int id)
        {
            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforeDiscountRemoval";
            transaction.CreateSavepoint(savepoint);
            try
            {
                var discount = await DbContext.Discount.SingleOrDefaultAsync(d => d.ID == id);
                if (discount == null)
                    return NotFound();

                var productsWithDiscount = DbContext.Product
                    .Where(d => d.DiscountID == id)
                    .ToList();

                //begins tracking products to avoid FK constraint errors
                DbContext.Product.UpdateRange(productsWithDiscount);

                DbContext.Discount.Remove(discount);
                await DbContext.SaveChangesAsync();

                transaction.Commit();

                return Ok(ResponseTemplate.Deleted(id.ToString(), nameof(Discount)));
            } catch (Exception ex)
            {
                transaction.RollbackToSavepoint(savepoint);
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
                return StatusCode(500, ResponseTemplate.InternalServerError());
            }

        }
    }
}
