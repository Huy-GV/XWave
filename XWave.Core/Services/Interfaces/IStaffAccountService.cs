using XWave.Core.DTOs.Management;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface IStaffAccountService
{
    Task<ServiceResult<StaffAccountDto>> GetStaffAccountById(string id, string managerId);

    Task<ServiceResult<IReadOnlyCollection<StaffAccountDto>>> GetAllStaffAccounts(string managerId);

    Task<ServiceResult<string>> RegisterStaffAccount(
        string staffId,
        string managerId,
        StaffAccountViewModel registerStaffViewModel);

    Task<ServiceResult> UpdateStaffAccount(string staffId, string managerId, StaffAccountViewModel updateUserViewModel);

    Task<ServiceResult> DeactivateStaffAccount(string staffId, string managerId);
}