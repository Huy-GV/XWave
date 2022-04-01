using System.Threading.Tasks;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        Task<ServiceResult> CancelJobAsync(object jobId);

        // todo: change schedule date
    }
}