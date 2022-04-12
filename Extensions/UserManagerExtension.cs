using Microsoft.AspNetCore.Identity;

namespace XWave.Extensions
{
    public static class UserManagerExtension
    {
        public static async System.Threading.Tasks.Task<bool> CheckPasswordAndLockoutStatusAsync<TUser>(
            this UserManager<TUser> userManager,
            TUser user,
            string password) where TUser : IdentityUser
        {
            if (user.LockoutEnabled)
            {
                return false;
            }

            return await userManager.CheckPasswordAsync(user, password);
        }
    }
}