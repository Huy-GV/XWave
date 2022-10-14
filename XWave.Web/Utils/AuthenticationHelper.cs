using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using XWave.Core.Data.Constants;

namespace XWave.Web.Utils;

public class AuthenticationHelper
{
    public AuthenticationHelper()
    {
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

    public CookieOptions CreateCookieOptions(int durationInDays, bool isSecure = true, bool isHttpOnly = true)
    {
        return new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(durationInDays),
            Secure = isSecure,
            HttpOnly = isHttpOnly
        };
    }
}