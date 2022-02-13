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
        public async Task<ServiceResult> CreateStaffAccount(RegisterUserViewModel registerUserViewModel)
        {
            var user = new ApplicationUser
            {
                UserName = registerUserViewModel.Username,
                FirstName = registerUserViewModel.FirstName,
                LastName = registerUserViewModel.LastName,
                RegistrationDate = DateTime.UtcNow.Date,
            };

            var result = await _userManager.CreateAsync(user, registerUserViewModel.Password);
            if (result.Succeeded)
            {
                return ServiceResult.Success(user.Id);
            }

            return ServiceResult.Failure("Failed to register user");
        }

        public async Task<StaffAccountDTO> GetStaffAccountByID(string id)
        {
            var staffUser = await _userManager.FindByIdAsync(id);
            if (staffUser != null)
            {
                var accountDTO = new StaffAccountDTO
                {
                    AccountID = staffUser.Id,
                    FullName = $"{staffUser.FirstName} {staffUser.LastName}",
                    RegistrationDate = staffUser.RegistrationDate
                };

                return accountDTO;
            }

            return null;
        }

        public Task<IEnumerable<StaffAccountDTO>> GetAllStaffAccounts()
        {
            var staffUsers = _userManager.Users.ToList();
            var accountDTOs = new List<StaffAccountDTO>();
            foreach (var user in staffUsers)
            {
                var accountDTO = new StaffAccountDTO
                {
                    AccountID = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    RegistrationDate = user.RegistrationDate
                };

                accountDTOs.Add(accountDTO);
            }


            return Task.FromResult<IEnumerable<StaffAccountDTO>>(accountDTOs);
        }
    }
}
