using Microsoft.AspNetCore.Identity;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Implementations;

internal class StaffAccountService : ServiceBase, IStaffAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffAccountService(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager) : base(dbContext)
    {
        _userManager = userManager;
    }

    public Task<ServiceResult<string>> RegisterStaffAccount(
        string id,
        StaffAccountViewModel registerStaffViewModel)
    {
        try
        {
            DbContext.StaffAccount.Add(new StaffAccount
            {
                StaffId = id,
                ImmediateManagerId = registerStaffViewModel.ImmediateManagerId,
                ContractEndDate = registerStaffViewModel.ContractEndDate,
                HourlyWage = registerStaffViewModel.HourlyWage
            });

            return Task.FromResult(ServiceResult<string>.Success(id));
        }
        catch (Exception)
        {
            return Task.FromResult(ServiceResult<string>.DefaultFailure());
        }
    }

    public async Task<StaffAccountDto?> GetStaffAccountById(string id)
    {
        var staffUser = await _userManager.FindByIdAsync(id);
        if (staffUser is not null)
        {
            return await BuildStaffAccountDto(staffUser);
        };

        return null;
    }

    public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAccounts()
    {
        var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.Staff);
        var staffAccountDtos = await Task.WhenAll(staffUsers
            .Select(async x => await BuildStaffAccountDto(x)));

        // filter null elements
        return staffAccountDtos.OfType<StaffAccountDto>();
    }

    public async Task<ServiceResult> UpdateStaffAccount(string staffId,
        StaffAccountViewModel updateStaffAccountViewModel)
    {
        try
        {
            var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
            if (staffAccount is null)
            {
                return ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = $"Staff account with ID {staffId} not found."
                });
            }

            DbContext.StaffAccount
                .Update(staffAccount)
                .CurrentValues
                .SetValues(updateStaffAccountViewModel);
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch
        {
            return ServiceResult.DefaultFailure();
        }
    }

    public async Task<ServiceResult> DeactivateStaffAccount(string staffId)
    {
        var staffUser = await _userManager.FindByIdAsync(staffId);
        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffUser is null || staffAccount is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Staff account with ID {staffId} not found."
            });
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var lockoutResult = await _userManager.SetLockoutEnabledAsync(staffUser, true);
            var lockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(staffUser, DateTime.MaxValue);
            var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(staffUser);
            var resetPasswordResult = await _userManager.ResetPasswordAsync(
                staffUser,
                resetPasswordToken,
                Guid.NewGuid().ToString());

            if (!lockoutResult.Succeeded || !lockoutEndDateResult.Succeeded || !resetPasswordResult.Succeeded)
            {
                return ServiceResult.DefaultFailure();
            }

            staffAccount.SoftDelete();
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Success();
        }
        catch
        {
            await transaction.RollbackAsync();

            return ServiceResult.DefaultFailure();
        }
    }

    private async Task<StaffAccountDto?> BuildStaffAccountDto(ApplicationUser staffUser)
    {
        var staffAccount = await DbContext.StaffAccount.FindAsync(staffUser.Id);
        if (staffAccount is null) return null;

        var manager = await _userManager.FindByIdAsync(staffAccount.ImmediateManagerId);
        var managerFullName = manager is null
            ? string.Empty
            : $"{manager.FirstName} {manager.LastName}";

        return new StaffAccountDto
        {
            StaffId = staffUser.Id,
            FullName = $"{staffUser.FirstName} {staffUser.LastName}",
            ContractStartDate = staffAccount.ContractStartDate,
            ContractEndDate = staffAccount.ContractEndDate,
            HourlyWage = staffAccount.HourlyWage,
            ImmediateManagerFullName = managerFullName
        };
    }
}