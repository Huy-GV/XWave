using System.Threading.Tasks;
using System;
using XWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XWave.Data;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.Extensions.Options;
using XWave.Data.Constants;

namespace XWave.Data.DatabaseSeeding
{
    public static class RoleSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                    serviceProvider
                    .GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                await CreateRolesAsync(
                    roleManager, 
                    new string[] 
                    {
                        Roles.Customer,
                        Roles.Manager,
                        Roles.Staff
                    }
                );

                await CreateCustomerAsync(userManager);
                await CreateStaffAsync(userManager);
                await CreateManagerAsync(userManager);
            } 
        }
        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager, string[] roles)
        {
            foreach(var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        private static async Task CreateCustomerAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("john_customer") != null) 
                return;

            var customer = new ApplicationUser()
            {
                UserName = "john_customer",
                RegistrationDate = DateTime.Now,
                Country = "Australia"
            };

            await userManager.CreateAsync(customer, "Password123@@");
            await userManager.AddToRoleAsync(customer, Roles.Customer);
        }
        private static async Task CreateStaffAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("paul_staff") != null) 
                return;

            var staff = new ApplicationUser()
            {
                UserName = "paul_staff",
                RegistrationDate = DateTime.Now,
                Country = "Australia"
            };

            await userManager.CreateAsync(staff, "Password123@@");
            await userManager.AddToRoleAsync(staff, Roles.Staff);
        }
        private static async Task CreateManagerAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("huy_manager") != null) 
                return;
            
            var manager = new ApplicationUser()
            {
                UserName = "huy_manager",
                RegistrationDate = DateTime.Now,
                Country = "Australia"
            };

            await userManager.CreateAsync(manager, "Password123@@");
            await userManager.AddToRoleAsync(manager, Roles.Manager);
        }
    }
}