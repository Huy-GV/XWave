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
            using var context = new XWaveDbContext(
                    serviceProvider
                    .GetRequiredService<DbContextOptions<XWaveDbContext>>());
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<XWaveDbContext>();
            await CreateRolesAsync(
                roleManager,
                new string[]
                {
                        Roles.Customer,
                        Roles.Manager,
                        Roles.Staff
                }
            );

            await CreateCustomersAsync(userManager, dbContext);
            await CreateStaffAsync(userManager);
            await CreateManagerAsync(userManager);
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
        private static async Task CreateCustomersAsync(
            UserManager<ApplicationUser> userManager,
            XWaveDbContext dbContext)
        {
            var customer1 = new ApplicationUser()
            {
                UserName = "john_customer",
                FirstName = "John",
                LastName = "Applebee",
                RegistrationDate = DateTime.Now
            };
            var customer2 = new ApplicationUser()
            {
                UserName = "jake_customer",
                FirstName = "Jake",
                LastName = "Applebee",
                RegistrationDate = DateTime.Now
            };


            await CreateCustomerAsync(customer1, userManager, dbContext);
            await CreateCustomerAsync(customer2, userManager, dbContext);
        }
        private static async Task CreateCustomerAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager,
            XWaveDbContext dbContext)
        {
            var result = await userManager.CreateAsync(user, "Password123@@");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.Customer);
                dbContext.Customer.Add(new Customer()
                {
                    CustomerID = user.Id,
                    Country = "Australia",
                    PhoneNumber = 98765432,
                    Address = "15 Second St VIC"
                });
                await dbContext.SaveChangesAsync();
            }
            
        }
        private static async Task CreateStaffAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByNameAsync("paul_staff") != null) 
                return;

            var staff = new ApplicationUser()
            {
                UserName = "paul_staff",
                FirstName = "Paul",
                LastName = "Applebee",
                RegistrationDate = DateTime.Now,
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
                FirstName = "Huy",
                LastName = "Applebee",
                RegistrationDate = DateTime.Now
            };

            await userManager.CreateAsync(manager, "Password123@@");
            await userManager.AddToRoleAsync(manager, Roles.Manager);
        }
    }
}