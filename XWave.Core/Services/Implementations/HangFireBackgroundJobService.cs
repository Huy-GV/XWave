using Hangfire;
using System.Linq.Expressions;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class HangFireBackgroundJobService : IBackgroundJobService
{
    public Task<string> AddBackgroundJobAsync(Expression<Func<Task>> function, DateTimeOffset schedule)
    {
        return Task.FromResult(BackgroundJob.Schedule(function, schedule));
    }

    public Task<ServiceResult> CancelJobAsync(object jobId)
    {
        if (jobId is string hangfireJobId)
            return Task.FromResult(BackgroundJob.Delete(hangfireJobId)
                ? ServiceResult.Success()
                : ServiceResult.Failure($"Failed to remove background job ID {hangfireJobId}."));

        return Task.FromResult(ServiceResult.Failure("Invalid scheduled job ID."));
    }
}