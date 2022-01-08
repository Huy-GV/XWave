using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.ViewModels.Management;

namespace XWave.ServiceInterfaces
{
    public interface IStaffActivityService
    {
        Task CreateLog(StaffActivityLogVM logVM, string staffID);
        Task<StaffActivityLog> GetActivityLog(int id);
        Task<IEnumerable<StaffActivityLog>> GetActivityLogs();
    }
}
