using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XWave.Configuration;
using XWave.Data;
using XWave.Data.Constants;
using XWave.Extensions;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;

namespace XWave.Services.Defaults
{
    public class JwtAuthenticationService : ServiceBase, IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Jwt _jwt;

        public JwtAuthenticationService(
            UserManager<ApplicationUser> userManager,
            IOptions<Jwt> jwt,
            XWaveDbContext dbContext) : base(dbContext)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthenticationResult> SignInAsync(SignInViewModel model)
        {
            var authenticationResult = new AuthenticationResult();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return authenticationResult with { Error = $"User with {model.Username} does not exist" };
            }

            if (!await _userManager.CheckPasswordAndLockoutStatusAsync(user, model.Password))
            {
                return authenticationResult with { Error = $"Unable to sign in user {model.Username}." };
            }

            var role = (await _userManager.GetRolesAsync(user)).First();
            var token = await CreateJwtTokenAsync(user, role);

            return new AuthenticationResult()
            {
                Succeeded = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
            };
        }

        public Task<AuthenticationResult> SignOutAsync(string userId)
        {
            return Task.FromResult(new AuthenticationResult() { Token = string.Empty });
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        private async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user, string role)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);

            IEnumerable<Claim> claims;
            claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // todo: add iat
                new Claim(CustomClaimType.UserId, user.Id),

                // todo: add roles through middleware, use jwt for authentication only
                new Claim(ClaimTypes.Role, role)
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(
                symmetricSecurityKey,
                SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims.Union(userClaims),
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        public async Task<AuthenticationResult> RegisterUserAsync(RegisterUserViewModel registerUserViewModel)
        {
            var appUser = new ApplicationUser
            {
                UserName = registerUserViewModel.Username,
                FirstName = registerUserViewModel.FirstName,
                LastName = registerUserViewModel.LastName,
                RegistrationDate = DateTime.UtcNow.Date,
            };

            var result = await _userManager.CreateAsync(appUser, registerUserViewModel.Password);

            return new AuthenticationResult() { Succeeded = result.Succeeded };
        }

        public async Task<bool> UserExists(string userId)
        {
            return await _userManager.FindByIdAsync(userId) != null;
        }
    }
}