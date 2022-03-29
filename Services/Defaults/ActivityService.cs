using XWave.Models;
using XWave.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using XWave.ViewModels.Management;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class ActivityService : ServiceBase, IActivityService
    {
        private readonly ILogger _logger;

        public ActivityService(
            XWaveDbContext dbContext,
            ILogger<ActivityService> logger) : base(dbContext)
        {
            _logger = logger;
        }

        public async Task<ServiceResult> LogActivityAsync<T>(string staffId, OperationType operation) where T : IEntity
        {
            try
            {
                var newLog = new Activity
                {
                    OperationType = operation,
                    Time = DateTime.Now,
                    StaffId = staffId,
                    EntityType = typeof(T).Name
                };

                DbContext.Activity.Add(newLog);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to log activiy create for staff ID {staffId}");
                return ServiceResult.Failure(e.Message);
            }
        }

        public async Task<IEnumerable<Activity>> FindAllActivityLogsAsync()
        {
            return await DbContext.Activity.AsNoTracking().ToListAsync();
        }

        public async Task<Activity> FindActivityLogAsync(int id)
        {
            return await DbContext.Activity
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}