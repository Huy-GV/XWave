using System.Threading.Tasks;
using System;
using XWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XWave.Data.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using XWave.Data.ViewModels;
using System.Net;

namespace XWave.Services
{
    public class AuthenticationModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    
    }
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
        private readonly JWT _jwt;
        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwt,
            ILogger<AuthenticationService> logger
            ) 
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
        }
        public async Task<AuthenticationModel> GetTokenAsync(ApplicationUser user, string role = null)
        {
            AuthenticationModel authModel = new();
            JwtSecurityToken jwtSecurityToken = await CreateJwtTokenAsync(user);
            
            authModel.IsAuthenticated = true;
            authModel.Message = "User logged in";
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.UserName = user.UserName;
            authModel.Role = role;

            return authModel;
        }
        public async Task<AuthenticationModel> LogInAsync(LogInVM model)
        {
            AuthenticationModel authModel = new();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) 
            {
                authModel.IsAuthenticated = false;
                authModel.Message = $"User with {model.Username} does not exist";
                return authModel;
            }

            var correctPassword = await _userManager.CheckPasswordAsync(user, model.Password);
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

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
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
        public async Task<AuthenticationModel> RegisterAsync(RegisterVM model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username
            };
            await _userManager.CreateAsync(user, model.Password);
            await _userManager.AddToRoleAsync(user, Roles.Customer);
            return await GetTokenAsync(user);

        }
        // public bool ValidToken(string token)
        // {
        //     var key = Encoding.ASCII.GetBytes(_jwt.Key);
        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     try
        //     {
        //         tokenHandler.ValidateToken(token, new TokenValidationParameters
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = new SymmetricSecurityKey(key),
        //             ValidateIssuer = false,
        //             ValidateAudience = false,
        //             // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        //             ClockSkew = TimeSpan.Zero
        //         }, out SecurityToken validatedToken);

        //         return true;
        //     }
        //     catch
        //     {
        //         return false;
        //     }
        // }
    }
}