using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IActivityService
    {
        Task<ServiceResult> CreateLog<T>(string staffID, OperationType operationType) where T : IEntity;
        Task<Activity> GetActivityLogAsync(int id);
        Task<IEnumerable<Activity>> GetActivityLogsAsync();
    }
}
