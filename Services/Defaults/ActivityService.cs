using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Management;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class ActivityService : ServiceBase, IActivityService
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivityService(
            XWaveDbContext dbContext,
            ILogger<ActivityService> logger,
            UserManager<ApplicationUser> userManager) : base(dbContext)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<ServiceResult> LogActivityAsync<T>(
            string staffId,
            OperationType operation,
            string infoText) where T : IEntity
        {
            try
            {
                var newLog = new Activity
                {
                    OperationType = operation,
                    Timestamp = DateTime.Now,
                    UserId = staffId,
                    EntityType = typeof(T).Name,
                    Info = infoText
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

        public async Task<IEnumerable<ActivityLogDto>> FindAllActivityLogsAsync()
        {
            return (await DbContext.Activity
                .AsNoTracking()
                .ToListAsync())
                .Select(async (a) => new ActivityLogDto()
                {
                    Timestamp = a.Timestamp,
                    InfoText = await GenerateInfoText(a.UserId, a.Info)
                })
                .Select(t => t.Result);
        }

        public async Task<ActivityLogDto?> FindActivityLogAsync(int id)
        {
            var log = await DbContext.Activity.AsNoTracking().FirstAsync(a => a.Id == id);
            if (log != null)
            {
                return new ActivityLogDto()
                {
                    Timestamp = log.Timestamp,
                    InfoText = await GenerateInfoText(log.UserId, log.Info)
                };
            }

            return null;
        }

        private async Task<string> GenerateInfoText(string userId, string infoText)
        {
            var (userName, firstName) = await GetUserInfo(userId);
            return $"{firstName} ({userName}) {infoText.Trim()}.";
        }

        private async Task<(string UserName, string FirstName)> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ("deleted", "deleted");
            }

            return (user.UserName, user.FirstName);
        }
    }
}