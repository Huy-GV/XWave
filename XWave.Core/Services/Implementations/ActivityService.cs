using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class ActivityService : ServiceBase, IActivityService
{
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        XWaveDbContext dbContext,
        ILogger<ActivityService> logger) : base(dbContext)
    {
        _logger = logger;
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
        catch (Exception exception)
        {
            _logger.LogError($"Failed to log activiy create for staff ID {staffId}.");
            _logger.LogError($"Exception message: {exception}.");
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
            return new ActivityLogDto
            {
                Id = log.Id,
                Timestamp = log.Timestamp,
                InfoText = CreateInfoText(log.AppUser, log.Info)
            };

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