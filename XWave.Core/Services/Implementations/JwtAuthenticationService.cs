using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XWave.Core.Configuration;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Authentication;

namespace XWave.Core.Services.Implementations;

internal class JwtAuthenticationService : ServiceBase, IAuthenticationService
{
    private readonly Jwt _jwt;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtAuthenticationService(
        UserManager<ApplicationUser> userManager,
        IOptions<Jwt> jwt,
        XWaveDbContext dbContext) : base(dbContext)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
    }

    public async Task<AuthenticationResult> SignInAsync(SignInViewModel viewModel)
    {
        var authenticationResult = new AuthenticationResult();
        var user = await _userManager.FindByNameAsync(viewModel.Username);
        var errorMessage = $"User with {viewModel.Username} does not exist";
        if (user == null ||
            !await _userManager.CheckPasswordAndLockoutStatusAsync(user, viewModel.Password))
            return authenticationResult with { Error = errorMessage };

        var token = CreateJwtTokenAsync(user);
        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthenticationResult
        {
            Succeeded = true,
            Token = serializedToken
        };
    }

    public Task<AuthenticationResult> SignOutAsync(string userId)
    {
        return Task.FromResult(new AuthenticationResult { Token = string.Empty });
    }

    public async Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel)
    {
        var appUser = new ApplicationUser
        {
            UserName = registerUserViewModel.Username,
            FirstName = registerUserViewModel.FirstName,
            LastName = registerUserViewModel.LastName,
            RegistrationDate = DateTime.UtcNow.Date
        };

        var result = await _userManager.CreateAsync(appUser, registerUserViewModel.Password);

        return new AuthenticationResult { Succeeded = result.Succeeded };
    }

    public async Task<bool> UserExists(string userId)
    {
        return await _userManager.FindByIdAsync(userId) != null;
    }

    public async Task<string[]> GetRoles(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return (await _userManager.GetRolesAsync(user)).ToArray();
    }

    private JwtSecurityToken CreateJwtTokenAsync(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            new Claim(XWaveClaimNames.UserId, user.Id)
        };

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(
            symmetricSecurityKey,
            SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials);

        return jwtSecurityToken;
    }
}