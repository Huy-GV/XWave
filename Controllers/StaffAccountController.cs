using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Management;
using XWave.Services.Interfaces;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="manager")]
    public class StaffAccountController : Controller
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
        public async Task<ActionResult<StaffAccountDto>> GetStaffAccountByID(string id)
        {
            return Ok(await _staffAccountService.GetStaffAccountById(id));
        }
    }
}
