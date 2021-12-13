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
using XWave.Data;
using System.Security.Principal;

namespace XWave.Services
{
    public class JWT
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationInMinutes { get; set; }
    }
    public class AuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly XWaveDbContext _dbContext;
        private readonly JWT _jwt;
        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwt,
            ILogger<AuthenticationService> logger,
            XWaveDbContext dbContext
            ) 
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
            _dbContext = dbContext;
        }
        private async Task<AuthenticationVM> GetTokenAsync
            (ApplicationUser user, 
            string role)
        {
            var token = await CreateJwtTokenAsync(user, role);
            AuthenticationVM authModel = new()
            {
                IsSuccessful = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
            };

            return authModel;
        }
        public async Task<AuthenticationVM> LogInAsync(LogInVM model)
        {
            AuthenticationVM authModel = new();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) 
            {
                authModel.Error = $"User with {model.Username} does not exist";
                return authModel;
            }

            var correctPassword = await _userManager
                .CheckPasswordAsync(user, model.Password);
            if (!correctPassword)
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
        public async Task<AuthenticationVM> RegisterAsync(RegisterVM registerVM, string role)
        {
            var user = new ApplicationUser
            {
                UserName = registerVM.Username,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                RegistrationDate = DateTime.UtcNow.Date,
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                if (role == Roles.Customer)
                    await CreateCustomerAccount(user.Id, registerVM);

                return await GetTokenAsync(user, role);
            }

            var errorMessage = "";
            foreach (var error in result.Errors)
                errorMessage += error.Description;

            return new AuthenticationVM
            {
                Error = errorMessage,
            };
        }
        public string GetCustomerID(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;

            string customerID = claimsIdentity?.FindFirst(CustomClaim.CustomerID)?.Value;
            _logger.LogInformation($"Customer id in jwt claim: {customerID}");
            return customerID ?? string.Empty;
        }
        private async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user, string role)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
        
            IEnumerable<Claim> claims;
            if (role == Roles.Customer)
            {
                claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("uid", user.Id),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(CustomClaim.CustomerID, user.Id)
                };
            } else
            {
                claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("uid", user.Id),
                    new Claim(ClaimTypes.Role, role)
                };
            }

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
        private async Task CreateCustomerAccount(string userID, RegisterVM registerVM)
        {
            _dbContext.Customer.Add(new Customer()
            {
                CustomerID = userID,
                Country = registerVM.Country,
                PhoneNumber = registerVM.PhoneNumber,
                Address = registerVM.Address,
            });
            await _dbContext.SaveChangesAsync();
        }

    }
}