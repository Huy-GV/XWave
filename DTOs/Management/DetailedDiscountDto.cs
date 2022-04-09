using System;

namespace XWave.DTOs.Management
{
    public record DetailedDiscountDto
    {
        public string ManagerName { get; set; }
        public uint Percentage { get; set; }
        public bool IsActive => EndDate > DateTime.Today && DateTime.Today > StartDate;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}