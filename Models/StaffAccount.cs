using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class StaffAccount
    {
        [Key]
        public string StaffId { get; set; }  
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public uint Salary { get; set; }
        public uint HoursPerWeek { get; set; }
        public string CreatorManagerId { get; set; }
        public ApplicationUser CreatorManager { get; set; }    
    }
}
