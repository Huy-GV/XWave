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
    [Route("api/staff-activity")]
    [Authorize(Roles ="manager")]
    public class StaffActivityLogController : AbstractController<StaffActivityLogController>
    {
        private readonly IStaffActivityService _staffActivityService;
        public StaffActivityLogController(
            IStaffActivityService staffActivityService,
            ILogger<StaffActivityLogController> logger) : base(logger)
        {
            _staffActivityService = staffActivityService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> Get()
        {
            return Ok(await _staffActivityService.GetActivityLogsAsync());
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActivityLog>> Get(int id)
        {
            return Ok(await _staffActivityService.GetActivityLogAsync(id));
        }
    }
}
