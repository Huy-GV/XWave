using Microsoft.AspNetCore.Authorization;
using XWave.Data.RequestModels ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Services;

namespace XWave.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        private readonly ILogger<AuthenticationController> _logger;
        private readonly AuthenticationService _authService;
        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            AuthenticationService authenticationService
            
            )
        {
            _logger = logger;
            _authService = authenticationService;  
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationModel>> RegisterAsync(RegisterModel model)
        {
            return await _authService.RegisterAsync(model);
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationModel>> LogInAsync(LogInModel model)
        {
            return await _authService.LogInAsync(model);
        }
        // [HttpGet("test/manager")]
        // [Authorize(Roles ="manager")]
        // public ActionResult<string> TestManager()
        // {
        //     return "OK MANAGER WORKS";
        // }
        // [HttpGet("test/staff")]
        // [Authorize(Roles = "staff")]
        // public ActionResult<string> TestStaff()
        // {
        //     return "OK STAFF WORKS";
        // }
        // [HttpGet("test")]
        // public ActionResult<string> GetUsers()
        // {
        //     return "OK WORKS";
        // }

        // [Authorize]
        // [HttpGet("test/random")]
        // public ActionResult<string> TestRandom()
        // {
        //     return "OK WORKS";
        // }
    }
}
