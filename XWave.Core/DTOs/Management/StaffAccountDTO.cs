namespace XWave.Core.DTOs.Management;

public record StaffAccountDto
{
    public string StaffId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime ContractStartDate { get; init; }
    public DateTime ContractEndDate { get; init; }

    public int RemainingDaysInContract
    {
        get
        {
            if (DateTime.Now > ContractEndDate) return 0;

            return (ContractEndDate - DateTime.Now).Days;
        }
    }

    public uint HourlyWage { get; init; }
    public string ImmediateManagerFullName { get; init; } = string.Empty;
}