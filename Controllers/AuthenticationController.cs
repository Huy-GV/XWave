
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
using XWave.Services.Interfaces;
using XWave.Configuration;
using XWave.Models;
using XWave.Services.ResultTemplate;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace XWave.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : AbstractController<AuthenticationController>
    {
        private readonly IAuthenticationService _authService;
        private readonly JwtCookie _jwtCookieConfig;
        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            IAuthenticationService authenticationService,
            IOptions<JwtCookie> jwtCookieOptions) : base (logger)
        {
            _jwtCookieConfig = jwtCookieOptions.Value;
            _authService = authenticationService;  
        }
        //TODO: attach jwt to cookies in response
        [HttpPost("register/customer")]
        public async Task<ActionResult<AuthenticationResult>> RegisterCustomerAsync(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
              
            var result = await _authService.RegisterAsync(model, Roles.Customer);
            if (result.Succeeded)
            {
                return Ok(result);
            }
                
            return XWaveBadRequest(result.Error); 
        }
        [HttpPost("register/staff")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<AuthenticationResult>> RegisterStaffAsync(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model, Roles.Staff);
            if (result.Succeeded)
            {
                return Ok(result);
            }
                
            return XWaveBadRequest(result.Error);
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResult>> LogInAsync(SignInUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.SignInAsync(model);
            if (result.Succeeded)
            {
                if (Request.Cookies.ContainsKey(_jwtCookieConfig.Name))
                {
                    Response.Cookies.Delete(_jwtCookieConfig.Name);
                }
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(_jwtCookieConfig.DurationInDays),
                    Secure = true,
                    HttpOnly = true,
                };
                Response.Cookies.Append(_jwtCookieConfig.Name, result.Token, cookieOptions);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult> SignOutAsync()
        {
            if (Request.Cookies.ContainsKey(_jwtCookieConfig.Name))
            {
                Response.Cookies.Delete(_jwtCookieConfig.Name);
            }

            return NoContent();
        }
    }
}
