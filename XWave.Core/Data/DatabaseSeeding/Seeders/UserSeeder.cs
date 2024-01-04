using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using XWave.Core.Data.Constants;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding;

internal class UserSeeder
{
    public static async Task SeedDevelopmentDataAsync(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<XWaveDbContext>();

        try
        {
            await CreateRolesAsync(roleManager);
            await CreateCustomersAsync(userManager, dbContext);
            await CreateTestManagersAsync(userManager, configuration);
            await CreateStaffAsync(userManager, dbContext);
        }
        catch (Exception)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<UserSeeder>>();
            logger.LogError("An error occurred while seeding roles and users");
            throw;
        }
    }

    public static async Task SeedProductionDataAsync(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<XWaveDbContext>();

        var logger = serviceProvider.GetRequiredService<ILogger<UserSeeder>>();
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
        XWaveDbContext dbContext)
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
        XWaveDbContext dbContext)
    {
        var managers = await userManager.GetUsersInRoleAsync(RoleNames.Manager);

        var staffUsers = TestUserFactory.StaffUsers();
        foreach (var staffUser in staffUsers)
        {
            await CreateSingleStaffAsync(userManager, staffUser);
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
        ApplicationUser staff)
    {
        await userManager.CreateAsync(staff, "Password123@@");
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

    private static async Task CreateProductionManagersAsync(
        UserManager<ApplicationUser> userManager,
        ILogger<UserSeeder> logger,
        IConfiguration configuration)
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
