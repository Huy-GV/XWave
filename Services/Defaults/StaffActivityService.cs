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
    public class StaffActivityService : ServiceBase, IStaffActivityService
    {
        private readonly ILogger _logger;
        public StaffActivityService(
            XWaveDbContext dbContext,
            ILogger<StaffActivityService> logger) : base(dbContext) 
        {  
            _logger = logger;
        }
        public async Task<ServiceResult> CreateLog<T>(string staffId, OperationType operation) where T : IEntity
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

                _logger.LogInformation($"Staff ID {staffId} created entity of type {newLog.EntityType}");
                return ServiceResult.Success(newLog.Id.ToString());
            } 
            catch(Exception e)
            {
                _logger.LogError($"Failed to log activiy create for staff ID {staffId}");
                return ServiceResult.Failure(e.Message);
            }
        }
        public async Task<IEnumerable<Activity>> GetActivityLogsAsync()
        {
            return await DbContext.Activity.AsNoTracking().ToListAsync();
        }
        public async Task<Activity> GetActivityLogAsync(int id)
        {
            return await DbContext.Activity
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
