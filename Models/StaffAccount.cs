using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class StaffAccount 
    {
        [Key]
        public string StaffId { get; set; }  
        [DataType("date")]
        public DateTime ContractEndDate { get; set; }
        public uint HourlyWage { get; set; }
        public string ImmediateManagerId { get; set; }
        public ApplicationUser ImmediateManager { get; set; }
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;
    }
}
