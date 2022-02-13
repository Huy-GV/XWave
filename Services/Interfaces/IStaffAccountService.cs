﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Management;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;

namespace XWave.Services.Interfaces
{
    public interface IStaffAccountService
    {
        public Task<StaffAccountDTO> GetStaffAccountByID(string id);
        public Task<IEnumerable<StaffAccountDTO>> GetAllStaffAccounts();
        public Task<ServiceResult> CreateStaffAccount(RegisterUserViewModel registerUserViewModel);
        //public Task<ServiceResult> UpdateStaffAccount(RegisterUserViewModel registerUserViewModel);
        //public Task<ServiceResult> DeleteStaffAccount(RegisterUserViewModel registerUserViewModel);
    }
}