using System.Threading.Tasks;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;

namespace XWave.Services.Interfaces;

public interface IAuthenticationService
{
    public Task<AuthenticationResult> SignInAsync(SignInViewModel signInViewModel);

    public Task<AuthenticationResult> SignOutAsync(string username);

    public Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel);

    public Task<bool> UserExists(string userId);
}