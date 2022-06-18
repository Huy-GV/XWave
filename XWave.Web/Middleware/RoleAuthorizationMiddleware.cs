using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using XWave.Core.Configuration;

namespace XWave.Web.Middleware;

public class RoleAuthorizationMiddleware : IMiddleware
{
    private readonly JwtCookie _jwtCookieOptions;
    private readonly Core.Services.Interfaces.IAuthenticationService _authenticationService;

    public RoleAuthorizationMiddleware(
        Core.Services.Interfaces.IAuthenticationService authenticationService,
        IOptions<JwtCookie> jwtCookieOptions)
    {
        _authenticationService = authenticationService;
        _jwtCookieOptions = jwtCookieOptions.Value;
    }

    // todo: why does ASP erase sub and name claims and add name id claim?
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.Cookies.Delete(_jwtCookieOptions.Name);
            await next.Invoke(context);
            return;
        }

        var userName = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userName))
        {
            context.Response.Cookies.Delete(_jwtCookieOptions.Name);
            await context.ChallengeAsync();
            return;
        }

        var roles = await _authenticationService.GetRoles(userName);
        if (roles.Length != 1)
        {
            await context.ForbidAsync();
            return;
        }

        var claim = new Claim(ClaimTypes.Role, roles.Single());
        var roleClaimIdentity = new ClaimsIdentity(new[] { claim });
        context.User.AddIdentity(roleClaimIdentity);
        await next(context);
    }
}