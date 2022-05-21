using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XWave.Configuration;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.Utils;
using XWave.ViewModels.Authentication;

namespace XWave.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IAuthenticationService _authService;
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

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> LogInAsync([FromBody] SignInViewModel model)
    {
        var result = await _authService.SignInAsync(model);
        if (!result.Succeeded) return Unauthorized(result);

        Response.Cookies.Delete(_jwtCookieConfig.Name);
        var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieConfig.DurationInDays);
        Response.Cookies.Append(_jwtCookieConfig.Name, result.Token, cookieOptions);

        return Ok(result);
    }

    [HttpPost]
    public ActionResult SignOutAsync()
    {
        Response.Cookies.Delete(_jwtCookieConfig.Name);

        return NoContent();
    }
}