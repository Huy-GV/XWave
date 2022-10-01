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

    public async Task<ServiceResult<string>> SignInAsync(SignInViewModel viewModel)
    {
        var user = await _userManager.FindByNameAsync(viewModel.Username);

        if (user is null)
        {
            return ServiceResult<string>.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"User with {viewModel.Username} not found",
            });
        }

        if (!await _userManager.CheckPasswordAndLockoutStatusAsync(user, viewModel.Password))
        {
            return ServiceResult<string>.Failure(new Error
            {
                ErrorCode = ErrorCode.InvalidUserRequest,
                Message = $"User with {viewModel.Username} locked out",
            });
        }

        var token = CreateJwtToken(user);
        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return ServiceResult<string>.Success(serializedToken);
    }

    public Task<ServiceResult> SignOutAsync(string userId)
    {
        return Task.FromResult(ServiceResult.Success());
    }

    public async Task<ServiceResult<string>> RegisterUserAsync(RegisterUserViewModel registerUserViewModel)
    {
        var appUser = new ApplicationUser
        {
            UserName = registerUserViewModel.UserName,
            FirstName = registerUserViewModel.FirstName,
            LastName = registerUserViewModel.LastName,
            RegistrationDate = DateTime.UtcNow.Date
        };

        var result = await _userManager.CreateAsync(appUser, registerUserViewModel.Password);
        if (result.Succeeded)
        {
            return ServiceResult<string>.Success(registerUserViewModel.UserName);
        }

        return ServiceResult<string>.Failure(new Error
        {
            ErrorCode = ErrorCode.Undefined,
            Message = string.Join(", ", result.Errors)
        }); ;
    }

    public async Task<bool> UserExists(string userId)
    {
        return await _userManager.FindByIdAsync(userId) is not null;
    }

    private JwtSecurityToken CreateJwtToken(ApplicationUser user)
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