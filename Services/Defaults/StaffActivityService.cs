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
    public class StaffActivityService : IStaffActivityService
    {
        private readonly XWaveDbContext _dbContext;
        private readonly ILogger<StaffActivityService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public StaffActivityService(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        { 
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public async Task CreateLog(StaffActivityLogVM logVM, string staffID)
        {
            if (!_dbContext.Activity.Any(a => a.ID == logVM.ActivityID)
                || await _userManager.FindByIdAsync(staffID) == null)
            {
                _logger.LogError("Activity or staff not found");
                return;
            }
            try
            {
                _dbContext.StaffActivityLog.Add(new StaffActivityLog
                { 
                    ActivityID = logVM.ActivityID,
                    Message = logVM.Message,
                    Time = DateTime.Now,
                    StaffID = staffID
                });

                await _dbContext.SaveChangesAsync();
            } 
            catch(Exception e)
            {
                _logger.LogError($"Failed to create log: {e.Message}");
            }
        }
        public async Task<IEnumerable<StaffActivityLog>> GetActivityLogsAsync()
        {
            return await _dbContext.StaffActivityLog.ToListAsync();
        }
        public async Task<StaffActivityLog> GetActivityLogAsync(int id)
        {
            return await _dbContext.StaffActivityLog.FirstOrDefaultAsync(a => a.ID == id);
        }
    }
}
