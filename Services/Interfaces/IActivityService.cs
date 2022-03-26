using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IActivityService
    {
        /// <summary>
        /// Log changes made by staff members.
        /// </summary>
        /// <typeparam name="T">Type of concerned entity.</typeparam>
        /// <param name="staffId">ID of staff member who made the change.</param>
        /// <param name="operationType">Operation made on the entity.</param>
        /// <returns></returns>
        Task<ServiceResult> LogActivityAsync<T>(string staffId, OperationType operationType) where T : IEntity;
        Task<Activity> FindActivityLogAsync(int id);
        Task<IEnumerable<Activity>> FindAllActivityLogsAsync();
    }
}
