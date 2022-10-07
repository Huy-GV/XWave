using Hangfire;
using System.Linq.Expressions;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class HangFireBackgroundJobService : IBackgroundJobService
{
    public Task<string> AddBackgroundJobAsync(Expression<Func<Task>> function, DateTimeOffset schedule)
    {
        // leaky abstraction because HangFire requires a public method
        return Task.FromResult(BackgroundJob.Schedule(function, schedule));
    }

    public Task<ServiceResult> CancelJobAsync(object jobId)
    {
        if (jobId is string hangfireJobId && BackgroundJob.Delete(hangfireJobId))
        {
            return Task.FromResult(ServiceResult.Success());
        }

        return Task.FromResult(ServiceResult.Failure(new Error
        {
            Code = ErrorCode.EntityNotFound,
            Message = $"Failed to remove background job ID {jobId}."
        }));
    }
}