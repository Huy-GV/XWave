using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.Constants;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Data.DatabaseSeeding.Seeders;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding;

internal class UserSeeder
{
    public static async Task SeedDevelopmentDataAsync<TSeeder>(
        XWaveDbContext context, 
        IConfiguration configuration, 
        RoleManager<IdentityRole> roleManager, 
        UserManager<ApplicationUser> userManager, 
        ILogger<TSeeder> logger) where TSeeder : IDataSeeder
    {
        try
        {
            await CreateRolesAsync(roleManager);
            await CreateCustomersAsync(userManager, context, configuration);
            await CreateTestManagersAsync(userManager, configuration);
            await CreateStaffAsync(userManager, context, configuration);
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while seeding roles and users");
            throw;
        }
    }

    public static async Task SeedProductionDataAsync<TSeeder>(
        XWaveDbContext context,
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<TSeeder> logger) where TSeeder : IDataSeeder
    {
        try
        {
            await CreateRolesAsync(roleManager);
            await CreateProductionManagersAsync(userManager, logger, configuration);
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while seeding roles and users");
            throw;
        }
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { RoleNames.Customer, RoleNames.Manager, RoleNames.Staff };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task CreateCustomersAsync(
        UserManager<ApplicationUser> userManager,
        XWaveDbContext dbContext,
        IConfiguration configuration)
    {
        var customer1 = new ApplicationUser
        {
            UserName = "john_customer",
            FirstName = "John",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        var customer2 = new ApplicationUser
        {
            UserName = "jake_customer",
            FirstName = "Jake",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        await CreateCustomerAsync(customer1, userManager, dbContext, configuration);
        await CreateCustomerAsync(customer2, userManager, dbContext, configuration);
    }

    private static async Task CreateCustomerAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        XWaveDbContext dbContext,
        IConfiguration configuration)
    {
        var password = configuration.GetValue<string>("SeedData:Password");
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, RoleNames.Customer);
            dbContext.CustomerAccount.Add(new CustomerAccount
            {
                CustomerId = user.Id,
                BillingAddress = "2 Main St, Sydney"
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task CreateStaffAsync(
        UserManager<ApplicationUser> userManager,
        XWaveDbContext dbContext,
        IConfiguration configuration)
    {
        var managers = await userManager.GetUsersInRoleAsync(RoleNames.Manager);

        var staffUsers = TestUserFactory.StaffUsers();
        foreach (var staffUser in staffUsers)
        {
            await CreateSingleStaffAsync(userManager, staffUser, configuration);
        }

        var staffAccounts = TestUserFactory.StaffAccounts(managers, staffUsers);
        foreach (var staffAccount in staffAccounts)
        {
            dbContext.StaffAccount.Add(staffAccount);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateSingleStaffAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser staff,
        IConfiguration configuration)
    {
        var password = configuration.GetValue<string>("SeedData:Password");
        await userManager.CreateAsync(staff, password);
        await userManager.AddToRoleAsync(staff, RoleNames.Staff);
    }

    private static async Task CreateTestManagersAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var managers = TestUserFactory.Managers();
        foreach (var manager in managers)
        {
            await CreateManagerAsync(manager, userManager, configuration);
        }
    }

    private static async Task CreateProductionManagersAsync<TSeeder>(
        UserManager<ApplicationUser> userManager,
        ILogger<TSeeder> logger,
        IConfiguration configuration) where TSeeder : IDataSeeder
    {
        var manager = new ApplicationUser
        {
            UserName = "admin",
            FirstName = "Admin",
            LastName = "Administrator",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "00000000"
        };

        if (!await userManager.Users.AnyAsync(x => x.UserName == manager.UserName))
        {
            logger.LogInformation("Seeding manager user in production");
            await CreateManagerAsync(manager, userManager, configuration);
        }
        else
        {
            logger.LogWarning("Manager user already seeded in production");
        }
    }

    private static async Task CreateManagerAsync(
        ApplicationUser manager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var password = configuration.GetValue<string>("SeedData:Password");

        await userManager.CreateAsync(manager, password);
        await userManager.AddToRoleAsync(manager, RoleNames.Manager);
    }
}
