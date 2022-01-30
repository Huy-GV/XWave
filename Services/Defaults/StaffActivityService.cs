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

namespace XWave.Services.Defaults
{
    //TODO: inherit from service base and apply this service to respective controller
    public class StaffActivityService : IStaffActivityService
    {
        private readonly XWaveDbContext _dbContext;
        private readonly ILogger<StaffActivityService> _logger;
        public StaffActivityService(
            XWaveDbContext dbContext)
        { 
            _dbContext = dbContext;
        }
        public async Task CreateLog<T>(string staffID, ActionType actionType) where T : IEntity
        {
            try
            {
                _dbContext.StaffActivityLog.Add(new ActivityLog
                { 
                    ActionType = actionType,
                    Time = DateTime.Now,
                    StaffID = staffID,
                    EntityType = typeof(T).Name
                });

                await _dbContext.SaveChangesAsync();
            } 
            catch(Exception e)
            {
                _logger.LogError($"Failed to create log: {e.Message}");
            }
        }
        public async Task<IEnumerable<ActivityLog>> GetActivityLogsAsync()
        {
            return await _dbContext.StaffActivityLog.ToListAsync();
        }
        public async Task<ActivityLog> GetActivityLogAsync(int id)
        {
            return await _dbContext.StaffActivityLog.FirstOrDefaultAsync(a => a.ID == id);
        }
    }
}
