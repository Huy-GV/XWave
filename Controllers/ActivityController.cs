using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;

namespace XWave.Controllers
{
    [ApiController]
    [Route("api/activity")]
    [Authorize(Roles ="manager")]
    public class ActivityController : AbstractController<ActivityController>
    {
        private readonly IStaffActivityService _staffActivityService;
        public ActivityController(
            IStaffActivityService staffActivityService,
            ILogger<ActivityController> logger) : base(logger)
        {
            _staffActivityService = staffActivityService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> Get()
        {
            return Ok(await _staffActivityService.GetActivityLogsAsync());
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Activity>> Get(int id)
        {
            return Ok(await _staffActivityService.GetActivityLogAsync(id));
        }
    }
}
