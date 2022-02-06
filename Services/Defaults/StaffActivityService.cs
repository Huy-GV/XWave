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
        public StaffActivityService(XWaveDbContext dbContext) : base(dbContext) {  }
        public async Task<ServiceResult> CreateLog<T>(string staffID, ActionType actionType) where T : IEntity
        {
            //TODO: return service result
            try
            {
                var newLog = new ActivityLog
                {
                    ActionType = actionType,
                    Time = DateTime.Now,
                    StaffID = staffID,
                    EntityType = typeof(T).Name
                };
                DbContext.StaffActivityLog.Add(newLog);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success(newLog.ID.ToString());
            } 
            catch(Exception e)
            {
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
