using XWave.Core.DTOs.Management;
using XWave.Core.Services.ResultTemplate;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface IStaffAccountService
{
    public Task<StaffAccountDto?> GetStaffAccountById(string id);

    public Task<IEnumerable<StaffAccountDto>> GetAllStaffAccounts();

    public Task<(ServiceResult, string? StaffId)> RegisterStaffAccount(string staffId,
        StaffAccountViewModel registerStaffViewModel);

    public Task<ServiceResult> UpdateStaffAccount(string staffId, StaffAccountViewModel updateUserViewModel);

    public Task<ServiceResult> DeactivateStaffAccount(string staffId);
}