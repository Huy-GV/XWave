using System.Security.Principal;
using System.Threading.Tasks;
using XWave.ViewModels.Authentication;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<AuthenticationResult> SignInAsync(SignInViewModel signInViewModel);
        public Task<AuthenticationResult> SignOutAsync(string username);
        public Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel);
        public Task<bool> IsUserInRoleAsync(string userId, string role);
    }
}
