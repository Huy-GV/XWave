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
        public async Task<ServiceResult> CreateLog<T>(string staffID, OperationType operation) where T : IEntity
        {
            try
            {
                var newLog = new ActivityLog
                {
                    OperationType = operation,
                    Time = DateTime.Now,
                    StaffID = staffID,
                    EntityType = typeof(T).Name
                };
                DbContext.StaffActivityLog.Add(newLog);
                await DbContext.SaveChangesAsync();

                _logger.LogInformation($"Staff ID {staffID} created entity of type {newLog.EntityType}");
                return ServiceResult.Success(newLog.ID.ToString());
            } 
            catch(Exception e)
            {
                _logger.LogError($"Failed to log activiy create for staff ID {staffID}");
                return ServiceResult.Failure(e.Message);
            }
        }
        public async Task<IEnumerable<ActivityLog>> GetActivityLogsAsync()
        {
            return await DbContext.StaffActivityLog.ToListAsync();
        }
        public async Task<ActivityLog> GetActivityLogAsync(int id)
        {
            return await DbContext.StaffActivityLog.FirstOrDefaultAsync(a => a.ID == id);
        }
    }
}
