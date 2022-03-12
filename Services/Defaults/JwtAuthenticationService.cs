using System.Threading.Tasks;
using System;
using XWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using XWave.Data.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using XWave.ViewModels.Authentication;
using XWave.Services.ResultTemplate;
using XWave.Services.Interfaces;
using XWave.Data;
using System.Security.Principal;
using XWave.Configuration;

namespace XWave.Services.Defaults
{
    public class JwtAuthenticationService : ServiceBase, IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Jwt _jwt;
        public JwtAuthenticationService(
            UserManager<ApplicationUser> userManager,
            IOptions<Jwt> jwt,
            ILogger<JwtAuthenticationService> logger,
            XWaveDbContext dbContext) : base(dbContext)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }
        public async Task<AuthenticationResult> SignInAsync(SignInUserViewModel model)
        {
            AuthenticationResult authModel = new();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) 
            {
                authModel.Error = $"User with {model.Username} does not exist";
                return authModel;
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Error = $"Incorrect password for user {model.Username}";
                return authModel;
            } else 
            {
                string role = (await _userManager.GetRolesAsync(user)).First();
                return await GetTokenAsync(user, role);
            }
        }
        public Task<AuthenticationResult> SignOutAsync(string userId)
        {
            return Task.FromResult(new AuthenticationResult() { Token = string.Empty });
        }
        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? false : await _userManager.IsInRoleAsync(user, role);
        }
        public async Task<AuthenticationResult> RegisterAsync(RegisterUserViewModel registerViewModel, string role)
        {
            var user = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                RegistrationDate = DateTime.UtcNow.Date,
            };

            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                if (role == Roles.Customer)
                {
                    await CreateCustomerAccount(user.Id, registerViewModel);
                }
                 
                return await GetTokenAsync(user, role);
            }

            var errorMessage = "";
            foreach (var error in result.Errors)
            {
                errorMessage += $"{error.Description}\n";
            }
                

            return new AuthenticationResult
            {
                Error = errorMessage,
            };
        }
        private async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user, string role)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
        
            IEnumerable<Claim> claims;
            claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(CustomClaimType.UserId, user.Id),
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
        private async Task CreateCustomerAccount(string userId, RegisterUserViewModel registerViewModel)
        {
            DbContext.Customer.Add(new CustomerAccount()
            {
                CustomerId = userId,
                //PhoneNumber = registerViewModel.PhoneNumber,
                //Address = registerViewModel.Address,
            });
            await DbContext.SaveChangesAsync();
        }
        private async Task<AuthenticationResult> GetTokenAsync (
            ApplicationUser user,
            string role)
        {
            var token = await CreateJwtTokenAsync(user, role);
            AuthenticationResult result = new()
            {
                Succeeded = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
            };

            return result;
        }
    }
}