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
[Authorize(Roles = nameof(Roles.Manager))]
public class StaffAccountController : ControllerBase
{
    private readonly IStaffAccountService _staffAccountService;
    private readonly AuthenticationHelper _authenticationHelper;

    public StaffAccountController(
        IStaffAccountService staffAccountService,
        AuthenticationHelper authenticationHelper)
    {
        _staffAccountService = staffAccountService;
        _authenticationHelper = authenticationHelper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffAccountDto>>> GetStaffAccounts()
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.GetAllStaffAccounts(managerId);
        return result.OnSuccess(Ok(result.Value));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StaffAccountDto>> GetStaffAccountById(string id)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.GetStaffAccountById(id, managerId);
        return result.OnSuccess(Ok(result.Value));
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<StaffAccountDto>> RegisterStaffAccount(string staffId, StaffAccountViewModel viewModel)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.RegisterStaffAccount(staffId, managerId, viewModel);
        return result.OnSuccess(Ok(result.Value));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResult>> UpdateStaffAccount(
        string staffAccountId,
        StaffAccountViewModel staffAccountViewModel)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.UpdateStaffAccount(
            staffAccountId, 
            managerId, 
            staffAccountViewModel);

        return result.OnSuccess(
            this.Updated($"{this.ApiUrl()}/staff-account/{staffAccountId}")); 
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServiceResult>> DeactivateStaffAccount(string staffAccountId)
    {
        var managerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _staffAccountService.DeactivateStaffAccount(staffAccountId, managerId);
        return result.OnSuccess(
            this.Updated($"{this.ApiUrl()}/staff-account/{staffAccountId}")); 
    }
}