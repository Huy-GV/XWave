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

namespace XWave.Services.Defaults
{
    // TODO: refactor this
    public class JWT
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationInMinutes { get; set; }
    }
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JwtAuthenticationService> _logger;
        private readonly XWaveDbContext _dbContext;
        private readonly JWT _jwt;
        public JwtAuthenticationService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwt,
            ILogger<JwtAuthenticationService> logger,
            XWaveDbContext dbContext
            ) 
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
            _dbContext = dbContext;
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
                //TODO: get one role only
                string role = (await _userManager.GetRolesAsync(user))[0];
                return await GetTokenAsync(user, role);
            }
        }
        public async Task<AuthenticationResult> SignOutAsync(string userID)
        {
            return new AuthenticationResult() { Token = string.Empty };
        }
        public async Task<bool> IsUserInRoleAsync(string userID, string role)
        {
            var user = await _userManager.FindByIdAsync(userID);
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
                    await CreateCustomerAccount(user.Id, registerViewModel);

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
                new Claim(CustomClaimType.UserID, user.Id),
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
        private async Task CreateCustomerAccount(string userID, RegisterUserViewModel registerViewModel)
        {
            _dbContext.Customer.Add(new Customer()
            {
                CustomerID = userID,
                Country = registerViewModel.Country,
                PhoneNumber = registerViewModel.PhoneNumber,
                Address = registerViewModel.Address,
            });
            await _dbContext.SaveChangesAsync();
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