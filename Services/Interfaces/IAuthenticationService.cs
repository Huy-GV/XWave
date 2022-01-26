using System.Security.Principal;
using System.Threading.Tasks;
using XWave.ViewModels.Authentication;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public string GetUserID(IIdentity itentity);
        public string GetUserName(IIdentity itentity);
        public Task<AuthenticationResult> SignInAsync(SignInVM signInVM);
        public Task<AuthenticationResult> RegisterAsync(RegisterVM registerVM, string role);
        public Task<AuthenticationResult> SignOutAsync(string username);
        public Task<bool> IsUserInRoleAsync(string userID, string role);
    }
}
