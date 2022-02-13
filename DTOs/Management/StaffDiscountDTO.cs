using System;

namespace XWave.DTOs.Management
{
    public class StaffDiscountDTO
    {
        public string ManagerID { get; set; }
        public string ManagerName { get; set; } 
        public uint Percentage { get; set; }
        public bool IsActive => EndDate > StartDate;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
