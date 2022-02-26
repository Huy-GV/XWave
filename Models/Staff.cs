using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class Staff
    {
        public string StaffId { get; set; }  
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public uint Salary { get; set; }
        public uint HoursPerWeek { get; set; }
        public string ManagerId { get; set; }
        public ApplicationUser Manager { get; set; }    
    }
}
