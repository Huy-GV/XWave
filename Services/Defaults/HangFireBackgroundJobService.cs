using Hangfire;
using System.Threading.Tasks;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class HangFireBackgroundJobService : IBackgroundJobService
    {
        public Task<ServiceResult> CancelJobAsync(object jobId)
        {
            if (jobId is string hangfireJobId)
            {
                return Task.FromResult(BackgroundJob.Delete(hangfireJobId)
                    ? ServiceResult.Success()
                    : ServiceResult.Failure($"Failed to remove background job ID {hangfireJobId}."));
            }

            return Task.FromResult(ServiceResult.Failure("Invalid scheduled job ID."));
        }
    }
}