using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Management;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces;

public interface IActivityService
{
    /// <summary>
    ///     Log changes made by staff members. Is exception-safe.
    /// </summary>
    /// <typeparam name="T">Type of concerned entity.</typeparam>
    /// <param name="staffId">ID of staff member who made the change.</param>
    /// <param name="operationType">Operation made on the entity.</param>
    /// <returns></returns>
    Task<ServiceResult> LogActivityAsync<T>(string staffId, OperationType operationType, string infoText)
        where T : IEntity;

    Task<ActivityLogDto?> FindActivityLogAsync(int id);

    Task<IEnumerable<ActivityLogDto>> FindAllActivityLogsAsync();
}