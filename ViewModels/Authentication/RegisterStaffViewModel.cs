﻿using System;
using System.ComponentModel.DataAnnotations;
using XWave.Models;
using XWave.ViewModels.Management;

namespace XWave.ViewModels.Authentication
{
    public class RegisterStaffViewModel
    {
        public RegisterUserViewModel User { get; set; }
        public StaffAccountViewModel StaffAccount { get; set; }
    }
}