using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XWave.Core.Data.Constants;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Authentication;
using XWave.Web.Utils;
using XWave.Core.Services.Communication;
using XWave.Web.Extensions;
using XWave.Web.Options;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerAccountController : ControllerBase
{
    private readonly IAuthenticationHelper _authenticationHelper;
    private readonly ICustomerAccountService _customerAccountService;
    private readonly JwtCookieOptions _jwtCookieOptionsOptions;

    public CustomerAccountController(
        IAuthenticationHelper authenticationHelper,
        ICustomerAccountService customerAccountService,
        IOptions<JwtCookieOptions> jwtCookieOptions)
    {
        _jwtCookieOptionsOptions = jwtCookieOptions.Value;
        _authenticationHelper = authenticationHelper;
        _customerAccountService = customerAccountService;
    }

    [HttpPost("register/customer")]
    [Authorize(Roles = nameof(RoleNames.Customer))]
    public async Task<ActionResult<ServiceResult<string>>> RegisterCustomerAsync(
        [FromBody] RegisterCustomerViewModel viewModel)
    {
        var result = await _customerAccountService.RegisterCustomerAsync(viewModel);
        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }

        if (Request.Cookies.ContainsKey(_jwtCookieOptionsOptions.Name))
        {
            Response.Cookies.Delete(_jwtCookieOptionsOptions.Name);
        }

        var cookieOptions = _authenticationHelper.CreateCookieOptions();
        Response.Cookies.Append(_jwtCookieOptionsOptions.Name, result.Value!, cookieOptions);

        return Ok(result.Value);
    }

    [HttpPost("subscribe")]
    [Authorize(Roles = nameof(RoleNames.Customer))]
    public async Task<ActionResult<ServiceResult>> UpdatePromotionSubscriptionAsync([FromBody] bool isSubscribed = true)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _customerAccountService.UpdateSubscriptionAsync(customerId, isSubscribed);
        return result.OnSuccess(NoContent);
    }
}