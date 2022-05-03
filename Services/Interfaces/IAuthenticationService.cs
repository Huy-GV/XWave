using System.Threading.Tasks;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;

namespace XWave.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<AuthenticationResult> SignInAsync(SignInViewModel signInViewModel);

        public Task<AuthenticationResult> SignOutAsync(string username);

        public Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel);

        public Task<bool> UserExists(string userId);

        // todo: move this to authorization service?
        /// <summary>
        /// Check if a user exists and is in a specified role.
        /// </summary>
        /// <param name="userId">ID of user.</param>
        /// <param name="role">Name of role.</param>
        /// <returns>True if the user exists and is in the specified role.</returns>
        public Task<bool> IsUserInRoleAsync(string userId, string role);
    }
}