using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using XWave.Configuration;
using XWave.Data.Constants;
using XWave.Models;

namespace XWave.Middleware;

public class RoleAuthorizationMiddleware : IMiddleware
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtCookie _jwtCookieOptions; 
    public RoleAuthorizationMiddleware(
        UserManager<ApplicationUser> userManager, 
        IOptions<JwtCookie> jwtCookieOptions)
    {
        _userManager = userManager;
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

        var user = await _userManager.FindByNameAsync(userName);
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count != 1)
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