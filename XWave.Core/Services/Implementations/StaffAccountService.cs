using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IRoleAuthorizer _roleAuthorizer;

    private static readonly Error UnauthorizedOperationError = Error.With(
        ErrorCode.AuthorizationError, 
        "Only managers are authorized to manage staff accounts");

    public StaffAccountService(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IRoleAuthorizer roleAuthorizer) : base(dbContext)
    {
        _userManager = userManager;
        _roleAuthorizer = roleAuthorizer;
    }

    public async Task<ServiceResult<string>> RegisterStaffAccount(
        string staffId,
        string managerId,
        StaffAccountViewModel registerStaffViewModel)
    {
        if (! await IsManagerIdValid(managerId))
        {
            return ServiceResult<string>.Failure(UnauthorizedOperationError);
        }

        var user = await _userManager.FindByIdAsync(staffId);
        if (user is null)
        {
            return ServiceResult<string>.Failure(
                Error.With(
                    ErrorCode.EntityNotFound,
                    $"User ID {staffId} not found"));
        }

        DbContext.StaffAccount.Add(new StaffAccount
        {
            StaffId = user.Id,
            ImmediateManagerId = registerStaffViewModel.ImmediateManagerId,
            ContractEndDate = registerStaffViewModel.ContractEndDate,
            HourlyWage = registerStaffViewModel.HourlyWage
        });

        await DbContext.SaveChangesAsync();
        return ServiceResult<string>.Success(user.Id);
    }

    public async Task<ServiceResult<StaffAccountDto>> GetStaffAccountById(string id, string managerId)
    {
        if (!await IsManagerIdValid(managerId))
        {
            return ServiceResult<StaffAccountDto>.Failure(UnauthorizedOperationError);
        }

        var staffUser = await _userManager.FindByIdAsync(id);
        var staffAccount = await DbContext.StaffAccount.FindAsync(id);
        if (staffAccount is not null &&
            staffUser is not null &&
            await _roleAuthorizer.IsUserInRole(id, RoleNames.Staff))
        {
            var staffAccountDto = await MapStaffAccountDto(staffUser, staffAccount);
            return ServiceResult<StaffAccountDto>.Success(staffAccountDto);
        };

        return ServiceResult<StaffAccountDto>.Failure(
            Error.With(
                ErrorCode.EntityNotFound,
                $"Staff account ID {id} not found"));
    }

    public async Task<ServiceResult<IReadOnlyCollection<StaffAccountDto>>> GetAllStaffAccounts(string managerId)
    {
        if (! await IsManagerIdValid(managerId))
        {
            return ServiceResult<IReadOnlyCollection<StaffAccountDto>>
                .Failure(UnauthorizedOperationError);
        }

        var staffUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Staff);
        var staffUserIds = staffUsers.Select(x => x.Id).ToArray();
        var staffAccounts = await DbContext.StaffAccount
            .Where(x => staffUserIds.Contains(x.StaffId))
            .ToArrayAsync();

        var staffAccountDtos = await Task.WhenAll(staffUsers
            .Zip(staffAccounts)
            .Select((tuple, _) => (User: tuple.First,Account: tuple.Second))
            .Select(async (tuple, _) => await MapStaffAccountDto(tuple.User, tuple.Account)));

        return ServiceResult<IReadOnlyCollection<StaffAccountDto>>.Success(staffAccountDtos.AsIReadonlyCollection());
    }

    public async Task<ServiceResult> UpdateStaffAccount(
        string staffId,
        string managerId,
        StaffAccountViewModel updateStaffAccountViewModel)
    {
        if (! await IsManagerIdValid(managerId))
        {
            return ServiceResult.Failure(UnauthorizedOperationError);
        }

        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffAccount is null)
        {
            return ServiceResult<StaffAccountDto>.Failure(
                Error.With(
                    ErrorCode.EntityNotFound,
                    $"Staff account ID {staffId} not found"));
        }

        DbContext.StaffAccount
            .Update(staffAccount)
            .CurrentValues
            .SetValues(updateStaffAccountViewModel);

        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeactivateStaffAccount(string staffId, string managerId)
    {
        if (! await IsManagerIdValid(managerId))
        {
            return ServiceResult.Failure(UnauthorizedOperationError);
        }

        var staffUser = await _userManager.FindByIdAsync(staffId);
        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffUser is null || staffAccount is null)
        {
            return ServiceResult<StaffAccountDto>.Failure(
                Error.With(
                    ErrorCode.EntityNotFound,
                    $"Staff account ID {staffId} not found"));
        }

        var lockoutResult = await _userManager.SetLockoutEnabledAsync(staffUser, true);
        var lockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(staffUser, DateTime.MaxValue);
        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(staffUser);
        var resetPasswordResult = await _userManager.ResetPasswordAsync(
            staffUser,
            resetPasswordToken,
            Guid.NewGuid().ToString());

        if (!lockoutResult.Succeeded || !lockoutEndDateResult.Succeeded || !resetPasswordResult.Succeeded)
        {
            return ServiceResult.UnknownFailure();
        }

        staffAccount.SoftDelete();
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private async Task<bool> IsManagerIdValid(string userId)
    {
        return await _roleAuthorizer.IsUserInRole(userId, RoleNames.Manager);
    }

    private async Task<StaffAccountDto> MapStaffAccountDto(ApplicationUser staffUser, StaffAccount staffAccount)
    {
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
