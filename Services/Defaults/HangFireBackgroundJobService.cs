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
            using (var connection = JobStorage.Current.GetConnection()) ;
            if (jobId is string hangfireJobId)
            {
                if (BackgroundJob.Delete(hangfireJobId))
                {
                    return Task.FromResult(ServiceResult.Success());
                }

                return Task.FromResult(ServiceResult.Failure($"Failed to remove background job ID {hangfireJobId}."));
            }
            else
            {
                return Task.FromResult(ServiceResult.Failure("Invalid scheduled job ID."));
            }
        }
    }
}