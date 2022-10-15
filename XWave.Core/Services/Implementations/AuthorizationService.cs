using Microsoft.AspNetCore.Identity;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class AuthorizationService : IAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string[]> GetRolesByUserId(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Array.Empty<string>();
        }

        return (await _userManager.GetRolesAsync(user)).ToArray();
    }

    public async Task<string[]> GetRolesByUserName(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return Array.Empty<string>();
        }

        return (await _userManager.GetRolesAsync(user)).ToArray();
    }

    public async Task<bool> IsUserInRole(string userId, string role)
    {
        return (await GetRolesByUserId(userId)).Contains(role);
    }

    public async Task<bool> IsUserInRoles(string userId, IEnumerable<string> roles)
    {
        return (await GetRolesByUserId(userId))
            .Intersect(roles)
            .Any();
    }
}