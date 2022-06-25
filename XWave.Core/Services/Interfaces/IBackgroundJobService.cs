using XWave.Core.Services.Communication;

namespace XWave.Core.Services.Interfaces;

public interface IBackgroundJobService
{
    Task<ServiceResult> CancelJobAsync(object jobId);

    // todo: change schedule date
    // todo: create model for scheduled background jobs
}