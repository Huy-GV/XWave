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

namespace XWave.Web.Controllers;

[Route("api/staff-account")]
[ApiController]
[Authorize(Roles = nameof(Roles.Manager))]
public class StaffAccountController : ControllerBase
{
    private readonly IStaffAccountService _staffAccountService;

    public StaffAccountController(IStaffAccountService staffAccountService)
    {
        _staffAccountService = staffAccountService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffAccountDto>>> GetStaffAccounts()
    {
        return Ok(await _staffAccountService.GetAllStaffAccounts());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StaffAccountDto>> GetStaffAccountById(string id)
    {
        var staffAccountDto = await _staffAccountService.GetStaffAccountById(id);
        return staffAccountDto is not null ? Ok(staffAccountDto) : NotFound();
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ServiceResult>> UpdateStaffAccount(string id,
        StaffAccountViewModel staffAccountViewModel)
    {
        var result = await _staffAccountService.UpdateStaffAccount(id, staffAccountViewModel);

        return result.MapResult(
            this.Updated($"https://localhost:5001/api/staff-account/{id}")); 
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServiceResult>> DeactivateStaffAccount(string id)
    {
        var result = await _staffAccountService.DeactivateStaffAccount(id);
        return result.MapResult(
            this.Updated($"https://localhost:5001/api/staff-account/{id}")); 
    }
}