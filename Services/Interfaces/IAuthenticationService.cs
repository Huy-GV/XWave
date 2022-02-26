using System.Security.Principal;
using System.Threading.Tasks;
using XWave.ViewModels.Authentication;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<AuthenticationResult> SignInAsync(SignInUserViewModel signInViewModel);
        public Task<AuthenticationResult> RegisterAsync(RegisterUserViewModel registerViewModel, string role);
        public Task<AuthenticationResult> SignOutAsync(string username);
        public Task<bool> IsUserInRoleAsync(string userId, string role);
    }
}
