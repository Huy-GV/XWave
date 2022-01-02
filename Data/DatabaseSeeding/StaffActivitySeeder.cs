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
            CreateActivitiesAsync(context, userManager).Wait(); 
            CreateStaffActivityLogsAsync(context, userManager).Wait();

            context.Database.CloseConnection();
        }
        private static async Task CreateActivitiesAsync(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            var managers = await userManager.GetUsersInRoleAsync(Roles.Manager);


            //TODO: throw instead of return
            if (managers.Count != 2)
                return;

            var activityTypes = new List<Activity>
            {
                new Activity()
                {
                    Name = "Product Stock Update",
                    CreationTime = DateTime.Now,
                    CreatingManagerID = managers[0].Id,
                    Description = "Occurs when a staff member alters the stock of a particular product"
                },
                 new Activity()
                {
                    Name = "Product Creation",
                    CreationTime = DateTime.Now,
                    CreatingManagerID = managers[0].Id,
                    Description = "Occurs when a staff member add a new product into the database"
                },
                 new Activity()
                {
                    Name = "Product Removal",
                    CreationTime = DateTime.Now,
                    CreatingManagerID = managers[1].Id,
                    Description = "Occurs when a staff member removes a product from the database"
                },
            };
            
            dbContext.Activity.AddRange(activityTypes);
            dbContext.SaveChanges();

        }
        private static async Task CreateStaffActivityLogsAsync(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            var activities = await dbContext.Activity.ToListAsync();
            var staff = await userManager.GetUsersInRoleAsync(Roles.Staff);

            //TODO: throw instead of return
            if (staff.Count != 1 || activities.Count != 3)
                return;

            var logs = new List<StaffActivityLog>
            {
                new StaffActivityLog
                {
                    Time = DateTime.Now,
                    Message = "Lorem ipsum dolor sed ema dislem bill loss. ",
                    StaffID = staff[0].Id,
                    ActivityID = activities[0].ID
                },
                new StaffActivityLog
                {
                    Time = DateTime.Now,
                    Message = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
                    StaffID = staff[0].Id,
                    ActivityID = activities[0].ID
                },
                new StaffActivityLog
                {
                    Time = DateTime.Now,
                    Message = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                    StaffID = staff[0].Id,
                    ActivityID = activities[0].ID
                },
            };
            dbContext.StaffActivityLog.AddRange(logs);
            dbContext.SaveChanges();
        }
    }
}
