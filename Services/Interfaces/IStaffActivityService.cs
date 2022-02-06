using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IStaffActivityService
    {
        Task<ServiceResult> CreateLog<T>(string staffID, ActionType actionType) where T : IEntity;
        Task<ActivityLog> GetActivityLogAsync(int id);
        Task<IEnumerable<ActivityLog>> GetActivityLogsAsync();
    }
}
