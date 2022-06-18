﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Services.Interfaces;
using XWave.Core.Services.ResultTemplate;
using XWave.Core.ViewModels.Authentication;
using XWave.Web.Configuration;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerAccountController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly ICustomerAccountService _customerAccountService;
    private readonly JwtCookie _jwtCookieOptions;

    public CustomerAccountController(
        AuthenticationHelper authenticationHelper,
        ICustomerAccountService customerAccountService,
        IOptions<JwtCookie> jwtCookieOptions)
    {
        _jwtCookieOptions = jwtCookieOptions.Value;
        _authenticationHelper = authenticationHelper;
        _customerAccountService = customerAccountService;
    }

    [HttpPost("register/customer")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult<AuthenticationResult>> RegisterCustomerAsync(
        [FromBody] RegisterCustomerViewModel viewModel)
    {
        var result = await _customerAccountService.RegisterCustomerAsync(viewModel);
        if (!result.Succeeded) return this.XWaveBadRequest(result.Error);

        if (Request.Cookies.ContainsKey(_jwtCookieOptions.Name)) Response.Cookies.Delete(_jwtCookieOptions.Name);

        var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieOptions.DurationInDays);
        Response.Cookies.Append(_jwtCookieOptions.Name, result.Token, cookieOptions);

        return Ok(result);
    }

    [HttpPost("subscribe")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult<ServiceResult>> UpdatePromotionSubscriptionAsync([FromBody] bool isSubscribed = true)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _customerAccountService.UpdateSubscriptionAsync(customerId, isSubscribed);
        if (result.Succeeded) return NoContent();

        return UnprocessableEntity(result.Errors);
    }
}