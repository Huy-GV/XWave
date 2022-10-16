using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.Constants;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding;

internal class UserSeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<XWaveDbContext>();

        try
        {
            CreateRolesAsync(roleManager).Wait();
            CreateCustomersAsync(userManager, dbContext).Wait();
            CreateStaffAsync(userManager).Wait();
            CreateManagersAsync(userManager).Wait();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<UserSeeder>>();
            logger.LogError("An error occurred while seeding roles and users");
        }
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { RoleNames.Customer, RoleNames.Manager, RoleNames.Staff };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
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

    private static async Task CreateStaffAsync(UserManager<ApplicationUser> userManager)
    {
        var staff1 = new ApplicationUser
        {
            UserName = "paul_staff",
            FirstName = "Paul",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };
        var staff2 = new ApplicationUser
        {
            UserName = "liz_staff",
            FirstName = "Elizabeth",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };
        await CreateSingleStaffAsync(userManager, staff1);
        await CreateSingleStaffAsync(userManager, staff2);
    }

    private static async Task CreateSingleStaffAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser staff)
    {
        await userManager.CreateAsync(staff, "Password123@@");
        await userManager.AddToRoleAsync(staff, RoleNames.Staff);
    }

    private static async Task CreateManagersAsync(
        UserManager<ApplicationUser> userManager)
    {
        var manager1 = new ApplicationUser
        {
            UserName = "gia_manager",
            FirstName = "Gia",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        var manager2 = new ApplicationUser
        {
            UserName = "huy_manager",
            FirstName = "Huy",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        await CreateManagerAsync(manager1, userManager);
        await CreateManagerAsync(manager2, userManager);
    }

    private static async Task CreateManagerAsync(
        ApplicationUser manager,
        UserManager<ApplicationUser> userManager)
    {
        await userManager.CreateAsync(manager, "Password123@@");
        await userManager.AddToRoleAsync(manager, RoleNames.Manager);
    }
}