using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Data.Constants;
using XWave.DTOs.Management;
using XWave.Extensions;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class StaffAccountService : ServiceBase, IStaffAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffAccountService(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager) : base(dbContext)
        {
            _userManager = userManager;
        }

        public Task<(ServiceResult, string? StaffId)> RegisterStaffAccount(string id, StaffAccountViewModel registerStaffViewModel)
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
            if (staffUser != null)
            {
                return await BuildStaffAccountDto(staffUser);
            }

            return null;
        }

        public async Task<IEnumerable<StaffAccountDto>> GetAllStaffAccounts()
        {
            var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.Staff);
            var accountDtos = new List<StaffAccountDto>();
            StaffAccountDto? accountDto = null;
            foreach (var user in staffUsers)
            {
                accountDto = await BuildStaffAccountDto(user);
                if (accountDto == null)
                {
                    // log error
                    continue;
                }

                accountDtos.Add(accountDto);
            }

            return accountDtos;
        }

        private async Task<StaffAccountDto?> BuildStaffAccountDto(ApplicationUser staffUser)
        {
            var staffAccount = await DbContext.StaffAccount.FindAsync(staffUser.Id);
            if (staffAccount == null)
            {
                return null;
            }

            var managerFullName = string.Empty;
            var manager = await _userManager.FindByIdAsync(staffAccount.ImmediateManagerId);
            if (manager != null)
            {
                managerFullName = $"{manager.FirstName} {manager.LastName}";
            }

            return new StaffAccountDto
            {
                StaffId = staffUser.Id,
                FullName = $"{staffUser.FirstName} {staffUser.LastName}",
                ContractEndDate = staffAccount.ContractEndDate,
                HourlyWage = staffAccount.HourlyWage,
                ImmediateManagerFullName = managerFullName
            };
        }

        public async Task<ServiceResult> UpdateStaffAccount(string staffId, StaffAccountViewModel updateStaffAccountViewModel)
        {
            try
            {
                var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
                if (staffAccount == null)
                {
                    return ServiceResult.Failure($"Staff account with ID {staffId} not found.");
                }
                
                DbContext.StaffAccount.Update(staffAccount).CurrentValues.SetValues(updateStaffAccountViewModel);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<ServiceResult> DeactivateStaffAccount(string staffId)
        {
            var staffUser = await _userManager.FindByIdAsync(staffId);
            if (staffUser == null)
            {
                return ServiceResult.Failure($"Unable to find user with id {staffId};");
            }

            var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
            if (staffAccount == null)
            {
                return ServiceResult.Failure($"Unable to find user with id {staffId}.");
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

                if (lockoutResult.Succeeded && lockoutEndDateResult.Succeeded && resetPasswordResult.Succeeded)
                {
                    staffAccount.SoftDelete();
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult.Success();
                }

                return ServiceResult.Failure("Deactivation process failed.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return ServiceResult.InternalFailure();
            }
        }
    }
}