using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Web.Data;
using XWave.Web.Extensions;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[ApiController]
[Route("api/activity")]
[Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
public class ActivityController : ControllerBase
{
    private readonly IStaffActivityLogger _staffActivityService;

    private readonly AuthenticationHelper _authenticationHelper;

    public ActivityController(
        IStaffActivityLogger staffActivityService,
        AuthenticationHelper authenticationHelper)
    {
        _staffActivityService = staffActivityService;
        _authenticationHelper = authenticationHelper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Activity>>> Get()
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffActivityService.FindAllActivityLogsAsync(staffId);
        return result.OnSuccess(Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Activity>> Get(int id)
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffActivityService.FindActivityLogAsync(id, staffId);
        return result.OnSuccess(Ok);
    }
}