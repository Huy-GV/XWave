using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using XWave.Web.Options;

namespace XWave.Web.Middleware;

public class RoleAuthorizationMiddleware : IMiddleware
{
    private readonly JwtCookieOptions _jwtCookieOptionsOptions;
    private readonly Core.Services.Interfaces.IRoleAuthorizer _roleAuthorizer;

    public RoleAuthorizationMiddleware(
        Core.Services.Interfaces.IRoleAuthorizer roleAuthorizer,
        IOptions<JwtCookieOptions> jwtCookieOptions)
    {
        _roleAuthorizer = roleAuthorizer;
        _jwtCookieOptionsOptions = jwtCookieOptions.Value;
    }

    // todo: why does ASP erase sub and name claims and add name id claim?
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.Cookies.Delete(_jwtCookieOptionsOptions.Name);
            await next.Invoke(context);
            return;
        }

        var userName = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userName))
        {
            context.Response.Cookies.Delete(_jwtCookieOptionsOptions.Name);
            await context.ChallengeAsync();
            return;
        }

        var roles = await _roleAuthorizer.GetRolesByUserName(userName);
        if (roles.Length != 1)
        {
            await context.ForbidAsync();
            return;
        }

        var claim = new Claim(ClaimTypes.Role, roles.Single());
        var roleClaimIdentity = new ClaimsIdentity([claim]);
        context.User.AddIdentity(roleClaimIdentity);

        await next(context);
    }
}
