using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Authentication;

namespace XWave.Core.Services.Interfaces;

public interface IAuthenticationService
{
    public Task<AuthenticationResult> SignInAsync(SignInViewModel signInViewModel);

    public Task<AuthenticationResult> SignOutAsync(string userName);

    public Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel);

    public Task<bool> UserExists(string userId);

    public Task<string[]> GetRoles(string userName);
}