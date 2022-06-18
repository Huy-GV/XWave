using System.ComponentModel.DataAnnotations;

namespace XWave.Core.Models;

public class StaffAccount : ISoftDeletable, IEntity
{
    [Key] public string StaffId { get; set; }

    [DataType("date")] public DateTime ContractStartDate { get; set; } = DateTime.Now.Date;

    [DataType("date")] public DateTime ContractEndDate { get; set; }

    public uint HourlyWage { get; set; }
    public string ImmediateManagerId { get; set; }
    public ApplicationUser ImmediateManager { get; set; }

    [MaxLength(100)] public string Address { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    [DataType("date")] public DateTime? DeleteDate { get; set; } = null;
}