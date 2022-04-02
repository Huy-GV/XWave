using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Models;
using XWave.Services.Interfaces;

namespace XWave.Controllers
{
    [ApiController]
    [Route("api/activity")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public class ActivityController : XWaveBaseController
    {
        private readonly IActivityService _staffActivityService;

        public ActivityController(
            IActivityService staffActivityService)
        {
            _staffActivityService = staffActivityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> Get()
        {
            return Ok(await _staffActivityService.FindAllActivityLogsAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Activity>> Get(int id)
        {
            var activity = await _staffActivityService.FindActivityLogAsync(id);
            return activity == null ? Ok(activity) : NotFound();
        }
    }
}