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
    private readonly IAuthorizationService _authorizationService;

    private readonly Error _unauthorizedOperationError = new()
    {
        Code = ErrorCode.AuthorizationError,
        Message = "Only managers are authorized to manage staff accounts",
    };
    public StaffAccountService(
        XWaveDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public async Task<ServiceResult<string>> RegisterStaffAccount(
        string id,
        string managerId,
        StaffAccountViewModel registerStaffViewModel)
    {
        if (! await IsManagerIdValid(managerId)) 
        {
            return ServiceResult<string>.Failure(_unauthorizedOperationError);
        }

        try
        {
            DbContext.StaffAccount.Add(new StaffAccount
            {
                StaffId = id,
                ImmediateManagerId = registerStaffViewModel.ImmediateManagerId,
                ContractEndDate = registerStaffViewModel.ContractEndDate,
                HourlyWage = registerStaffViewModel.HourlyWage
            });

            await DbContext.SaveChangesAsync();
            return ServiceResult<string>.Success(id);
        }
        catch (Exception)
        {
            return ServiceResult<string>.UnknownFailure();
        }
    }

    public async Task<ServiceResult<StaffAccountDto>> GetStaffAccountById(string id, string managerId)
    {
        if (! await IsManagerIdValid(managerId)) 
        {
            return ServiceResult<StaffAccountDto>.Failure(_unauthorizedOperationError);
        }
        
        var staffUser = await _userManager.FindByIdAsync(id);
        var staffAccountDto = await MapStaffAccountDtoOrDefault(staffUser);
        if (staffAccountDto is not null)
        {
            return ServiceResult<StaffAccountDto>.Success(staffAccountDto);
        };

        return ServiceResult<StaffAccountDto>.Failure(new Error 
        {
            Code = ErrorCode.EntityNotFound,
        });
    }

    public async Task<ServiceResult<IReadOnlyCollection<StaffAccountDto>>> GetAllStaffAccounts(string managerId)
    {
        if (! await IsManagerIdValid(managerId)) 
        {
            return ServiceResult<IReadOnlyCollection<StaffAccountDto>>
                .Failure(_unauthorizedOperationError);
        }

        var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.Staff);
        var staffAccountDtos = await Task.WhenAll(staffUsers
            .Select(async x => await MapStaffAccountDtoOrDefault(x)));
            
        // filter null elements
        var readonlyCollection = staffAccountDtos
            .OfType<StaffAccountDto>()
            .ToList()
            .AsIReadonlyCollection();

        return ServiceResult<IReadOnlyCollection<StaffAccountDto>>.Success(readonlyCollection);
    }

    public async Task<ServiceResult> UpdateStaffAccount(
        string staffId,
        string managerId,
        StaffAccountViewModel updateStaffAccountViewModel)
    {
        if (! await IsManagerIdValid(managerId)) 
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffAccount is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Staff account with ID {staffId} not found."
            });
        }

        try
        {
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

    public async Task<ServiceResult> DeactivateStaffAccount(string staffId, string managerId)
    {
        if (! await IsManagerIdValid(managerId)) 
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var staffUser = await _userManager.FindByIdAsync(staffId);
        var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
        if (staffUser is null || staffAccount is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
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

    private async Task<bool> IsManagerIdValid(string userId)
    {
        var roles = await _authorizationService.GetRolesByUserId(userId);
        return roles.FirstOrDefault() == Roles.Manager;
    }

    private async Task<StaffAccountDto?> MapStaffAccountDtoOrDefault(ApplicationUser? staffUser)
    {
        if (staffUser is null)
        {
            return null;
        }

        var staffAccount = await DbContext.StaffAccount.FindAsync(staffUser.Id);
        if (staffAccount is null)
        {
            return null;
        }

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