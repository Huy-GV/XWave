using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Authentication
{
    public class UpdateStaffAccountViewModel
    {
        [Range(20, 500)]
        public uint HourlyWage { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string ImmediateManagerId { get; set; }
    }
}
