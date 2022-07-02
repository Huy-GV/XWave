using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Authentication;

namespace XWave.Core.Services.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Sign the user in.
    /// </summary>
    /// <param name="signInViewModel">Viewmodel containing the sign-in credentials.</param>
    /// <returns></returns>
    public Task<ServiceResult<string>> SignInAsync(SignInViewModel signInViewModel);

    /// <summary>
    /// Signs the user out.
    /// </summary>
    /// <param name="userName">User name of the user to sign out.</param>
    /// <returns></returns>
    public Task<ServiceResult> SignOutAsync(string userName);

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="registerUserViewModel">Viewmodel containing register credentials.</param>
    /// <returns>Service result containing the user name if registration is successful</returns>
    public Task<ServiceResult<string>> RegisterUserAsync(RegisterUserViewModel registerUserViewModel);

    public Task<bool> UserExists(string userId);

    public Task<string[]> GetRoles(string userName);
}