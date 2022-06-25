using XWave.Core.DTOs.Management;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface IStaffAccountService
{ 
    Task<StaffAccountDto?> GetStaffAccountById(string id);
    Task<IEnumerable<StaffAccountDto>> GetAllStaffAccounts();
    Task<(ServiceResult, string? StaffId)> RegisterStaffAccount(string staffId,
        StaffAccountViewModel registerStaffViewModel);
    Task<ServiceResult> UpdateStaffAccount(string staffId, StaffAccountViewModel updateUserViewModel);
    Task<ServiceResult> DeactivateStaffAccount(string staffId);
}