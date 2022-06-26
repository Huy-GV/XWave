using System.Linq.Expressions;
using XWave.Core.Services.Communication;

namespace XWave.Core.Services.Interfaces;

public interface IBackgroundJobService
{
    Task<ServiceResult> CancelJobAsync(object jobId);

    Task<string> AddBackgroundJobAsync(Expression<Func<Task>> function, DateTimeOffset schedule);

    // todo: change schedule date
    // todo: create model for scheduled background jobs
}