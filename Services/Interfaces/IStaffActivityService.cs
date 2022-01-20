﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.ViewModels.Management;

namespace XWave.Service.Interfaces
{
    public interface IStaffActivityService
    {
        Task CreateLog(StaffActivityLogVM logVM, string staffID);
        Task<StaffActivityLog> GetActivityLogAsync(int id);
        Task<IEnumerable<StaffActivityLog>> GetActivityLogsAsync();
    }
}