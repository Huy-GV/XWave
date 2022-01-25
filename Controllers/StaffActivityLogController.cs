﻿using Microsoft.AspNetCore.Mvc;
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
    public class StaffActivityLogController : AbstractController<StaffActivityLogController>
    {
        private readonly IStaffActivityService _staffActivityService;
        private readonly ILogger<StaffActivityLogController> _logger;
        public StaffActivityLogController(
            IStaffActivityService staffActivityService,
            ILogger<StaffActivityLogController> logger) : base(logger)
        {
            _staffActivityService = staffActivityService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffActivityLog>>> Get()
        {
            return Ok(await _staffActivityService.GetActivityLogsAsync());
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StaffActivityLog>> Get(int id)
        {
            return Ok(await _staffActivityService.GetActivityLogAsync(id));
        }
    }
}
