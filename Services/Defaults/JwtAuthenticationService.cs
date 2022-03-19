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
            XWaveDbContext dbContext) : base(dbContext)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }
        public async Task<AuthenticationResult> SignInAsync(SignInViewModel model)
        {
            var authModel = new AuthenticationResult();
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
        //public async Task<AuthenticationResult> RegisterStaffAsync(RegisterStaffViewModel viewModel)
        //{
        //    var appUser = viewModel.User;
        //    // todo: add transactions
        //    var user = new ApplicationUser
        //    {
        //        UserName = appUser.Username,
        //        FirstName = appUser.FirstName,
        //        LastName = appUser.LastName,
        //        RegistrationDate = DateTime.UtcNow.Date,
        //    };

        //    var result = await _userManager.CreateAsync(user, appUser.Password);
        //    if (result.Succeeded)
        //    {
        //        var staffAccount = viewModel.StaffAccount;
        //        await _staffAccountService.RegisterStaffAccount(user.Id, staffAccount);
        //        await _userManager.AddToRoleAsync(user, Roles.Staff);

        //        return await GetTokenAsync(user, Roles.Staff);
        //    }

        //    var errorMessage = "";
        //    foreach (var error in result.Errors)
        //    {
        //        errorMessage += $"{error.Description}\n";
        //    }
                

        //    return new AuthenticationResult
        //    {
        //        Error = errorMessage,
        //    };
        //}
       
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
            if (result.Succeeded)
            {
                return new AuthenticationResult()
                {
                    Succeeded = true
                };
            }

            return new AuthenticationResult()
            {
                Succeeded = false
            };
        }
    }
}