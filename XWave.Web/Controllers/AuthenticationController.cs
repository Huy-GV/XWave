using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XWave.Core.Configuration;
using XWave.Core.DTOs.Shared;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Authentication;
using XWave.Web.Extensions;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IAuthenticator _authenticator;
    private readonly JwtCookie _jwtCookieOptions;

    public AuthenticationController(
        IAuthenticator authenticator,
        AuthenticationHelper authenticationHelper,
        IOptions<JwtCookie> jwtCookieOptions)
    {
        _jwtCookieOptions = jwtCookieOptions.Value;
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

        Response.Cookies.Delete(_jwtCookieOptions.Name);
        var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieOptions);
        Response.Cookies.Append(_jwtCookieOptions.Name, result.Value, cookieOptions);

        return Ok(new JwtTokenDto { Token = result.Value });
    }

    [HttpPost]
    public ActionResult SignOutAsync()
    {
        Response.Cookies.Delete(_jwtCookieOptions.Name);

        return NoContent();
    }
}
