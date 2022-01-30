using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IStaffActivityService
    {
        Task CreateLog<T>(string staffID, ActionType actionType) where T : IEntity;
        Task<ActivityLog> GetActivityLogAsync(int id);
        Task<IEnumerable<ActivityLog>> GetActivityLogsAsync();
    }
}
