using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Management;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;
using System;
using Microsoft.EntityFrameworkCore;
using XWave.Data.Constants;
using System.Transactions;
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
        public async Task<ServiceResult> RegisterStaffAccount(string id, StaffAccountViewModel registerStaffViewModel)
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

                return ServiceResult.Success(id);
            }
            catch (Exception)
            {
                return ServiceResult.Failure("Failed to register user");
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
            var staffAccount = await DbContext.StaffAccount.FindAsync(staffId);
            try
            {
                var entry = DbContext.StaffAccount.Update(staffAccount);
                entry.CurrentValues.SetValues(updateStaffAccountViewModel);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success(staffId);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult> DeactivateStaffAccount(string staffId)
        {
            var staffUser = await _userManager.FindByIdAsync(staffId);
            if (staffUser == null)
            {
                return ServiceResult.Failure("User not found");
            }

            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                await _userManager.SetLockoutEnabledAsync(staffUser, true);
                await _userManager.SetLockoutEndDateAsync(staffUser, DateTime.MaxValue);
                var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(staffUser);
                var result = await _userManager.ResetPasswordAsync(staffUser, resetPasswordToken, Guid.NewGuid().ToString());
                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                    return ServiceResult.Success(staffId);
                }

                throw new Exception("Password reset failed");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return ServiceResult.Failure(ex.Message);
            }
        }
    }
}
