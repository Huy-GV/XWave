using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IActivityService
    {
        Task<ServiceResult> LogActivityAsync<T>(string staffId, OperationType operationType) where T : IEntity;
        Task<Activity> FindActivityLogAsync(int id);
        Task<IEnumerable<Activity>> FindAllActivityLogsAsync();
    }
}
