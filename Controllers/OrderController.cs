using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using XWave.Data;
using XWave.Data.Constants;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using XWave.Models;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : AbstractController<OrderController>
    {
        public OrderController(
            XWaveDbContext dbContext,
            ILogger<OrderController> logger) : base(dbContext, logger)
        {

        }
        //[Authorize]
        //public ActionResult<Order> GetAsync()
        //{

        //}
        //public async Task<IActionResult> CreateOrder([FromBody] )
        //{
        //    //receive auth model
        //}


        //TODO: validate before creating order
    }
}
