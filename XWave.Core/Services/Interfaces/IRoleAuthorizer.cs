namespace XWave.Core.Services.Interfaces;

public interface IRoleAuthorizer
{
    Task<string[]> GetRolesByUserName(string userName);

    Task<string[]> GetRolesByUserId(string userId);

    Task<bool> IsUserInRole(string userId, string role);

    Task<bool> IsUserInRoles(string userId, IEnumerable<string> roles);
}
