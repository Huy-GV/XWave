namespace XWave.Core.DTOs.Management;

public record StaffAccountDto
{
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }

    public int RemainingDaysInContract
    {
        get
        {
            if (DateTime.Now > ContractEndDate) return 0;

            return (ContractEndDate - DateTime.Now).Days;
        }
    }

    public uint HourlyWage { get; set; }
    public string ImmediateManagerFullName { get; set; } = string.Empty;
}