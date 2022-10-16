using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class AuthorizationService : ServiceBase, IAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    public AuthorizationService(
        UserManager<ApplicationUser> userManager,
        XWaveDbContext dbContext) : base(dbContext)
    {
        _userManager = userManager;
    }

    public async Task<string[]> GetRolesByUserId(string userId)
    {
        return await GetRolesByUserIdQuery(userId).ToArrayAsync();
    }

    public async Task<string[]> GetRolesByUserName(string userName)
    {
        var query = 
            from user in DbContext.Users
            join userRole in DbContext.UserRoles on user.Id equals userRole.UserId
            join role in DbContext.Roles on userRole.RoleId equals role.Id
            where user.UserName == userName
            select role.Name;  

        return await query.ToArrayAsync();
    }

    public async Task<bool> IsUserInRole(string userId, string role)
    {
        return await GetRolesByUserIdQuery(userId).AnyAsync(x => x == role);
    }

    public async Task<bool> IsUserInRoles(string userId, IEnumerable<string> roles)
    {
        return await GetRolesByUserIdQuery(userId).AnyAsync(x => roles.Contains(x));
    }

    private IQueryable<string> GetRolesByUserIdQuery(string userId)
    {
        return  DbContext.Roles
            .Join(DbContext.UserRoles,
                roles => roles.Id,
                userRoles => userRoles.RoleId,
                (role, userRole) => new 
                { 
                    RoleName = role.Name, 
                    UserId = userRole.UserId 
                })
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleName);
    }
}