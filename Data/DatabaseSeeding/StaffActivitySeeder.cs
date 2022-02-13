using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using XWave.Data.Constants;
using XWave.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XWave.Data.DatabaseSeeding
{
    public class StaffActivitySeeder
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
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding staff activities");
                logger.LogError(ex.Message);
            } finally
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

            if (staff.Count < 2 || managers.Count < 1)
            {
                throw new Exception("Insufficient staff or manager");
            }

            var logs = new List<ActivityLog>
            {
                new ActivityLog
                {
                    Time = DateTime.Now,
                    StaffID = staff[0].Id,
                    EntityType = typeof(Product).Name,
                    OperationType = OperationType.Create,
                },
                new ActivityLog
                {
                    Time = DateTime.Now,
                    StaffID = staff[1].Id,
                    EntityType = typeof(Product).Name,
                    OperationType = OperationType.Modify,
                },
                new ActivityLog
                {
                    Time = DateTime.Now,
                    StaffID = managers[0].Id,
                    EntityType = typeof(Product).Name,
                    OperationType = OperationType.Delete,
                },
            };
            dbContext.StaffActivityLog.AddRange(logs);
            dbContext.SaveChanges();
        }
    }
}
