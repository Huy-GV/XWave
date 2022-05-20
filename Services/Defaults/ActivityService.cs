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
        private readonly ILogger<ActivityService> _logger;
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
                _logger.LogError($"Failed to log activiy create for staff ID {staffId}.");
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<IEnumerable<ActivityLogDto>> FindAllActivityLogsAsync()
        {
            return await DbContext.Activity
                .AsNoTracking()
                .Include(x => x.AppUser)
                .Select(a => new ActivityLogDto
                {
                    Id = a.Id,
                    Timestamp = a.Timestamp,
                    InfoText = CreateInfoText(a.AppUser, a.Info)
                })
                .ToListAsync();
        }

        public async Task<ActivityLogDto?> FindActivityLogAsync(int id)
        {
            var log = await DbContext.Activity
                .Include(x => x.AppUser)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);
            
            if (log != null)
            {
                return new ActivityLogDto()
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    InfoText = CreateInfoText(log.AppUser, log.Info)
                };
            }

            return null;
        }

        private static string CreateInfoText(ApplicationUser? user, string infoText)
        {
            var (userName, firstName, lastName) = user == null 
                ? ("[Deleted]", string.Empty, "[Deleted]")
                : (user.UserName, user.FirstName, user.LastName);
            
            return $"{firstName} {lastName} ({userName}) {infoText.Trim()}.";
        }
    }
}