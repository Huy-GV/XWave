using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.Constants;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class StaffActivitySeeder
{
    public static async Task SeedData<TSeeder>(
        XWaveDbContext context, 
        UserManager<ApplicationUser> userManager, 
        ILogger<TSeeder> logger) where TSeeder : IDataSeeder
    {
        try
        {
            await CreateStaffActivityLogsAsync(context, userManager);
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while seeding staff activities");
            throw;
        }
    }

    private static async Task CreateStaffActivityLogsAsync(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var staff = await userManager.GetUsersInRoleAsync(RoleNames.Staff);
        var managers = await userManager.GetUsersInRoleAsync(RoleNames.Staff);

        if (staff.Count < 2 || managers.Count < 1)
        {
            throw new Exception("Insufficient staff or manager");
        }

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