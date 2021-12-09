using Microsoft.AspNetCore.Authorization;
using XWave.ViewModels.Authentication ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Services;
using XWave.Data.Constants;
using XWave.Data;
using XWave.Models;

namespace XWave.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : AbstractController<AuthenticationController>
    {
        private readonly AuthenticationService _authService;
        public AuthenticationController(
            XWaveDbContext dbContext,
            ILogger<AuthenticationController> logger,
            AuthenticationService authenticationService
            
            ) : base (dbContext, logger)
        {
            _authService = authenticationService;  
        }

        [HttpPost("register/customer")]
        public async Task<ActionResult<AuthenticationVM>> RegisterCustomerAsync(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authVM = await _authService.RegisterAsync(model, Roles.Customer);
            if (authVM.IsSuccessful)
                return Ok(authVM);
 
            return BadRequest(authVM.Error);
            
        }
        [HttpPost("register/staff")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<AuthenticationVM>> RegisterStaffAsync(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authModel = await _authService.RegisterAsync(model, Roles.Staff);
            if (authModel.IsSuccessful)
                return Ok(authModel);

            return BadRequest(authModel.Error);
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationVM>> LogInAsync(LogInVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _authService.LogInAsync(model);
        }

        [HttpGet("test/manager")]
        [Authorize(Roles ="manager")]
        public ActionResult<string> TestManager()
        {
            return "OK MANAGER WORKS";
        }
        [HttpGet("test/staff")]
        [Authorize(Roles = "staff")]
        public ActionResult<string> TestStaff()
        {
            return "OK STAFF WORKS";
        }
        [HttpGet("test")]
        public ActionResult<string> GetUsers()
        {
            return "OK WORKS";
        }
        [Authorize]
        [HttpGet("test/random")]
        public ActionResult<string> TestRandom()
        {
            return "OK WORKS";
        }
        [Authorize(Roles ="customer")]
        [HttpGet("test/customer")]
        public ActionResult<string> TestCustomer()
        {
            return "OK WORKS";
        }
        [Authorize(Policy ="StaffOnly")]
        [HttpGet("test/staffonly")]
        public ActionResult<string> StaffOnly()
        {
            return "OK WORKS";
        }
    }
}
