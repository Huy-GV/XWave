using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Management;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Management;
using XWave.Core.Services.Communication;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/staff-account")]
[ApiController]
[Authorize(Roles = nameof(RoleNames.Manager))]
public class StaffAccountController : ControllerBase
{
    private readonly IStaffAccountService _staffAccountService;
    private readonly IAuthenticationHelper _authenticationHelper;

    public StaffAccountController(
        IStaffAccountService staffAccountService,
        IAuthenticationHelper authenticationHelper)
    {
        _staffAccountService = staffAccountService;
        _authenticationHelper = authenticationHelper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffAccountDto>>> GetStaffAccounts()
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.GetAllStaffAccounts(managerId);
        return result.OnSuccess(Ok);
    }

    [HttpGet("{staffId}")]
    public async Task<ActionResult<StaffAccountDto>> GetStaffAccountById(string staffId)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.GetStaffAccountById(staffId, managerId);
        return result.OnSuccess(Ok);
    }

    [HttpPost("{staffId}")]
    public async Task<ActionResult<StaffAccountDto>> RegisterStaffAccount(string staffId, StaffAccountViewModel viewModel)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.RegisterStaffAccount(staffId, managerId, viewModel);
        return result.OnSuccess(Ok);
    }

    [HttpPut("{staffId}")]
    public async Task<ActionResult<ServiceResult>> UpdateStaffAccount(
        string staffId,
        StaffAccountViewModel staffAccountViewModel)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.UpdateStaffAccount(
            staffId,
            managerId,
            staffAccountViewModel);

        return result.OnSuccess(() =>
            this.Updated($"{this.ApiUrl()}/staff-account/{staffId}"));
    }

    [HttpDelete("{staffId}")]
    public async Task<ActionResult<ServiceResult>> DeactivateStaffAccount(string staffId)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.DeactivateStaffAccount(staffId, managerId);
        return result.OnSuccess(() =>
            this.Updated($"{this.ApiUrl()}/staff-account/{staffId}"));
    }
}