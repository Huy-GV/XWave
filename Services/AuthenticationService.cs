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
        public async Task<AuthenticationVM> GetTokenAsync
            (ApplicationUser user, 
            string role = null)
        {
            AuthenticationVM authModel = new();


            JwtSecurityToken jwtSecurityToken = await CreateJwtTokenAsync(user);
            
            authModel.IsAuthenticated = true;
            authModel.Message = "User logged in";
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.UserName = user.UserName;
            authModel.Role = role;

            return authModel;
        }
        public async Task<AuthenticationVM> LogInAsync(LogInVM model)
        {
            AuthenticationVM authModel = new();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) 
            {
                authModel.IsAuthenticated = false;
                authModel.Message = $"User with {model.Username} does not exist";
                return authModel;
            }

            var correctPassword = await _userManager
                .CheckPasswordAsync(user, model.Password);
            if (!correctPassword)
            {
                authModel.Message = $"Invalid password for user {model.Username}";
                return authModel;
            } else 
            {
                string role = (await _userManager.GetRolesAsync(user))[0];
                return await GetTokenAsync(user, role);
            }
        }
        private async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            foreach(var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }    

            //TODO: only add when user role is customer
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", user.Id),
                new Claim(CustomClaim.CustomerID, user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(
                symmetricSecurityKey, 
                SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
        private async Task CreateCustomerAccount(string userID)
        {
            _dbContext.Customer.Add(new Customer()
            {
                CustomerID = userID,
                Country = "Australia",
                PhoneNumber = 98765432,
                Address = "15 Second St VIC"
            });
            await _dbContext.SaveChangesAsync();
        }
        public async Task<AuthenticationVM> RegisterAsync(RegisterVM model, string role)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegistrationDate = DateTime.UtcNow.Date,
            };


            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                if (role == Roles.Customer)
                    await CreateCustomerAccount(user.Id);


                return await GetTokenAsync(user);
            }

            return new AuthenticationVM
            {
                IsAuthenticated = false,
                Message = "Invalid username or password",
                Token = null,
                UserName = user.UserName,
                Role = role,
            };
        }
    }
}