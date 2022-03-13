using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Management;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IStaffAccountService
    {
        public Task<StaffAccountDto?> GetStaffAccountById(string id);
        public Task<IEnumerable<StaffAccountDto>> GetAllStaffAccounts();
        public Task<ServiceResult> RegisterStaffAccount(string staffId, StaffAccountViewModel registerStaffViewModel);
        public Task<ServiceResult> UpdateStaffAccount(string staffId, StaffAccountViewModel updateUserViewModel);
        public Task<ServiceResult> DeactivateStaffAccount(string staffId);
    }
}
