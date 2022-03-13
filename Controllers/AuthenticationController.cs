
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
using XWave.Helpers;
using System.ComponentModel.DataAnnotations;

namespace XWave.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : XWaveBaseController
    {
        private readonly IAuthenticationService _authService;
        private readonly AuthenticationHelper _authenticationHelper;
        private readonly JwtCookie _jwtCookieConfig;
        public AuthenticationController(
            IAuthenticationService authenticationService,
            AuthenticationHelper authenticationHelper,
            IOptions<JwtCookie> jwtCookieOptions)
        {
            _jwtCookieConfig = jwtCookieOptions.Value;
            _authService = authenticationService;
            _authenticationHelper = authenticationHelper;
        }
        // todo: move this to user account controller
        [HttpPost("register/customer")]
        public async Task<ActionResult<AuthenticationResult>> RegisterCustomerAsync(AppUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
              
            //var result = await _authService.RegisterAsync(model, Roles.Customer);
            //if (result.Succeeded)
            //{
            //    if (Request.Cookies.ContainsKey(_jwtCookieConfig.Name))
            //    {
            //        Response.Cookies.Delete(_jwtCookieConfig.Name);
            //    }
            //    var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieConfig.DurationInDays);
            //    Response.Cookies.Append(_jwtCookieConfig.Name, result.Token, cookieOptions);
            //    return Ok(result);
            //}
                
            return XWaveBadRequest(result.Error); 
        }
        [HttpPost("register/staff")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<AuthenticationResult>> RegisterStaffAsync(RegisterStaffViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            var result = await _authService.RegisterAsync(viewModel.User, Roles.Staff);
            if (result.Succeeded)
            {
                return Ok(result);
            }
                
            return XWaveBadRequest(result.Error);
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResult>> LogInAsync(SignInViewModel model)
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
                var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieConfig.DurationInDays);
                Response.Cookies.Append(_jwtCookieConfig.Name, result.Token, cookieOptions);
            }

            return Ok(result);
        }
        [HttpPost]
        public ActionResult SignOutAsync()
        {
            if (Request.Cookies.ContainsKey(_jwtCookieConfig.Name))
            {
                Response.Cookies.Delete(_jwtCookieConfig.Name);
            }

            return NoContent();
        }
    }
}
