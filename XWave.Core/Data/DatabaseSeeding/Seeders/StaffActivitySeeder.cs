using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.Constants;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding;

internal class StaffActivitySeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        try
        {
            CreateStaffActivityLogsAsync(context, userManager).Wait();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<StaffActivitySeeder>>();
            logger.LogError(ex, "An error occurred while seeding staff activities");
            logger.LogError(ex.Message);
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }

    private static async Task CreateStaffActivityLogsAsync(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var staff = await userManager.GetUsersInRoleAsync(Roles.Staff);
        var managers = await userManager.GetUsersInRoleAsync(Roles.Staff);

        if (staff.Count < 2 || managers.Count < 1) throw new Exception("Insufficient staff or manager");

        var logs = new List<Activity>
        {
            new()
            {
                Timestamp = DateTime.Now,
                UserId = staff[0].Id,
                EntityType = typeof(Product).Name,
                OperationType = OperationType.Create,
                Info = " created a product named Test 1"
            },
            new()
            {
                Timestamp = DateTime.Now,
                UserId = staff[1].Id,
                EntityType = typeof(Product).Name,
                OperationType = OperationType.Modify,
                Info = " created a product named Test 2"
            },
            new()
            {
                Timestamp = DateTime.Now,
                UserId = managers[0].Id,
                EntityType = typeof(Product).Name,
                OperationType = OperationType.Delete,
                Info = " created a product named Test 3"
            }
        };

        dbContext.Activity.AddRange(logs);
        dbContext.SaveChanges();
    }
}