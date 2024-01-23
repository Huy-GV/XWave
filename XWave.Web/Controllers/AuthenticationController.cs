using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XWave.Core.DTOs.Shared;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Authentication;
using XWave.Web.Extensions;
using XWave.Web.Options;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationHelper _authenticationHelper;
    private readonly IAuthenticator _authenticator;
    private readonly JwtCookieOptions _jwtCookieOptionsOptions;

    public AuthenticationController(
        IAuthenticator authenticator,
        IAuthenticationHelper authenticationHelper,
        IOptions<JwtCookieOptions> jwtCookieOptions)
    {
        _jwtCookieOptionsOptions = jwtCookieOptions.Value;
        _authenticator = authenticator;
        _authenticationHelper = authenticationHelper;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ServiceResult<string>>> LogInAsync([FromBody] SignInViewModel model)
    {
        var result = await _authenticator.SignInAsync(model);
        if (!result.Succeeded)
        {
            return result.Error.MapToHttpResult();
        }

        Response.Cookies.Delete(_jwtCookieOptionsOptions.Name);
        var cookieOptions = _authenticationHelper.CreateCookieOptions();
        Response.Cookies.Append(_jwtCookieOptionsOptions.Name, result.Value, cookieOptions);

        return Ok(new JwtTokenDto { Token = result.Value });
    }

    [HttpPost]
    public ActionResult SignOutAsync()
    {
        Response.Cookies.Delete(_jwtCookieOptionsOptions.Name);

        return NoContent();
    }
}
