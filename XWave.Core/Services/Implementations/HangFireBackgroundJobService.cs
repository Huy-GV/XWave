using Hangfire;
using XWave.Core.Services.Interfaces;
using XWave.Core.Services.ResultTemplate;

namespace XWave.Core.Services.Implementations;

internal class HangFireBackgroundJobService : IBackgroundJobService
{
    public Task<ServiceResult> CancelJobAsync(object jobId)
    {
        if (jobId is string hangfireJobId)
            return Task.FromResult(BackgroundJob.Delete(hangfireJobId)
                ? ServiceResult.Success()
                : ServiceResult.Failure($"Failed to remove background job ID {hangfireJobId}."));

        return Task.FromResult(ServiceResult.Failure("Invalid scheduled job ID."));
    }
}