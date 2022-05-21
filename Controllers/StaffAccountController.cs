using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Data.Constants;
using XWave.DTOs.Management;
using XWave.Extensions;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Controllers;

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
        return staffAccountDto != null ? Ok(staffAccountDto) : NotFound();
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<ServiceResult>> UpdateStaffAccount(string id,
        StaffAccountViewModel staffAccountViewModel)
    {
        var result = await _staffAccountService.UpdateStaffAccount(id, staffAccountViewModel);
        if (result.Succeeded) return this.XWaveUpdated($"https://localhost:5001/api/staff-account/{id}");

        return UnprocessableEntity(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServiceResult>> DeactivateStaffAccount(string id)
    {
        var result = await _staffAccountService.DeactivateStaffAccount(id);
        return result.Succeeded
            ? this.XWaveUpdated($"https://localhost:5001/api/staff-account/{id}")
            : UnprocessableEntity(result.Errors);
    }
}