using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.Constants;

namespace XWave.Web.Utils;

public class AuthenticationHelper
{
    private ILogger<AuthenticationHelper> _logger;

    public AuthenticationHelper(ILogger<AuthenticationHelper> logger)
    {
        _logger = logger;
    }

    public string GetUserId(IIdentity? identity)
    {
        var claimsIdentity = identity as ClaimsIdentity;
        var userId = claimsIdentity?.FindFirst(XWaveClaimNames.UserId)?.Value ?? string.Empty;

        return userId;
    }

    public string GetUserName(IIdentity? identity)
    {
        var claimsIdentity = identity as ClaimsIdentity;
        var customerId = claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        return customerId;
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