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

    public Task<(ServiceResult, string? StaffId)> RegisterStaffAccount(string id,
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

            return Task.FromResult<(ServiceResult, string? StaffId)>((ServiceResult.Success(), id));
        }
        catch (Exception)
        {
            return Task.FromResult<(ServiceResult, string? StaffId)>((ServiceResult.InternalFailure(), null));
        }
    }

    public async Task<StaffAccountDto?> GetStaffAccountById(string id)
    {
        var staffUser = await _userManager.FindByIdAsync(id);
        if (staffUser != null) return await BuildStaffAccountDto(staffUser);

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
            if (staffAccount == null) return ServiceResult.Failure($"Staff account with ID {staffId} not found.");

            DbContext.StaffAccount.Update(staffAccount).CurrentValues.SetValues(updateStaffAccountViewModel);
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch
        {
            return ServiceResult.InternalFailure();
        }
    }

    public async Task<ServiceResult> DeactivateStaffAccount(string staffId)
    {
        var staffUser = await _userManager.FindByIdAsync(staffId);
        if (staffUser == null) return ServiceResult.Failure($"Unable to find user with id {staffId};");

        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffAccount == null) return ServiceResult.Failure($"Unable to find user with id {staffId}.");

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
                return ServiceResult.Failure("Deactivation process failed.");

            staffAccount.SoftDelete();
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Success();
        }
        catch
        {
            await transaction.RollbackAsync();

            return ServiceResult.InternalFailure();
        }
    }

    private async Task<StaffAccountDto?> BuildStaffAccountDto(ApplicationUser staffUser)
    {
        var staffAccount = await DbContext.StaffAccount.FindAsync(staffUser.Id);
        if (staffAccount == null) return null;

        var manager = await _userManager.FindByIdAsync(staffAccount.ImmediateManagerId);
        var managerFullName = manager == null
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