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
        public async Task<ServiceResult> RegisterStaffAccount(RegisterStaffViewModel registerStaffViewModel)
        {
            var user = new ApplicationUser
            {
                UserName = registerStaffViewModel.Username,
                FirstName = registerStaffViewModel.FirstName,
                LastName = registerStaffViewModel.LastName,
            };

            var result = await _userManager.CreateAsync(user, registerStaffViewModel.Password);
            if (result.Succeeded)
            {
                DbContext.StaffAccount.Add(new StaffAccount
                {
                    StaffId = user.Id,
                    ImmediateManagerId = registerStaffViewModel.ImmediateManagerId,
                    ContractEndDate = registerStaffViewModel.ContractEndDate,
                    HourlyWage = registerStaffViewModel.HourlyWage
                });

                return ServiceResult.Success(user.Id);
            }

            return ServiceResult.Failure("Failed to register user");
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
        public async Task<ServiceResult> UpdateStaffAccount(string staffId, UpdateStaffAccountViewModel updateStaffAccountViewModel)
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

            await _userManager.SetLockoutEnabledAsync(staffUser, true);
            await _userManager.SetLockoutEndDateAsync(staffUser, DateTime.MaxValue);

            return ServiceResult.Success(staffId);
        }
    }
}
