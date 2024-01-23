using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using XWave.Core.Data.Constants;
using XWave.Web.Options;

namespace XWave.Web.Utils;

public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly JwtCookieOptions _jwtCookieOptions;
    
    public AuthenticationHelper(IOptions<JwtCookieOptions> jwtCookieOptions)
    {
        _jwtCookieOptions = jwtCookieOptions.Value;
    }

    public string GetUserId(IIdentity? identity)
    {
        var claimsIdentity = identity as ClaimsIdentity;
        return claimsIdentity?.FindFirst(XWaveClaimNames.UserId)?.Value ?? string.Empty;
    }

    public string GetUserName(IIdentity? identity)
    {
        var claimsIdentity = identity as ClaimsIdentity;
        return claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public CookieOptions CreateCookieOptions()
    {
        return new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(_jwtCookieOptions.DurationInDays),
            Secure = true,
            HttpOnly = _jwtCookieOptions.HttpOnly
        };
    }
}