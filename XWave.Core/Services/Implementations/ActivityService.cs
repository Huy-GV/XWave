using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class ActivityService : ServiceBase, IActivityService
{
    private readonly ILogger<ActivityService> _logger;
    private readonly IAuthorizationService _authorizationService;

    public ActivityService(
        XWaveDbContext dbContext,
        ILogger<ActivityService> logger,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _logger = logger;
        _authorizationService = authorizationService;
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
            _logger.LogError($"Failed to log activity create for staff ID {staffId}.");
            _logger.LogError($"Exception message: {exception}.");
            return ServiceResult.DefaultFailure();
        }
    }

    public async Task<ServiceResult<IReadOnlyCollection<ActivityLogDto>>> FindAllActivityLogsAsync(string staffId)
    {
        if (! await IsStaffIdValid(staffId))
        {
            return ServiceResult<IReadOnlyCollection<ActivityLogDto>>.Failure(new Error
            {
                Code = ErrorCode.AuthenticationError
            });
        }

        var activityLogDtos = await DbContext.Activity
            .AsNoTracking()
            .Include(x => x.AppUser)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                InfoText = CreateInfoText(a.AppUser, a.Info)
            })
            .ToListAsync();

        return  ServiceResult<IReadOnlyCollection<ActivityLogDto>>.Success(activityLogDtos.AsIReadonlyCollection());
    }

    public async Task<ServiceResult<ActivityLogDto>> FindActivityLogAsync(int id, string staffId)
    {
        if (! await IsStaffIdValid(staffId))
        {
            return ServiceResult<ActivityLogDto>.Failure(new Error
            {
                Code = ErrorCode.AuthenticationError,
            });
        }

        var activityLog = await DbContext.Activity
            .Include(x => x.AppUser)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (activityLog is null)
        {
            return ServiceResult<ActivityLogDto>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
            });
        }

        var activityLogDto = new ActivityLogDto
        {
            Id = activityLog.Id,
            Timestamp = activityLog.Timestamp,
            InfoText = CreateInfoText(activityLog.AppUser, activityLog.Info)
        };

        return ServiceResult<ActivityLogDto>.Success(activityLogDto);
    }

    private async Task<bool> IsStaffIdValid(string userId)
    {
        var roles = await _authorizationService.GetRolesByUserId(userId);
        return roles.Intersect(new [] { Roles.Manager, Roles.Staff }).Any();
    }

    private static string CreateInfoText(ApplicationUser? user, string infoText)
    {
        var (userName, firstName, lastName) = user is null
            ? ("[Deleted]", string.Empty, "[Deleted]")
            : (user.UserName, user.FirstName, user.LastName);

        return $"{firstName} {lastName} ({userName}) {infoText.Trim()}.";
    }
}