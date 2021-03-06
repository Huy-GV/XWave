namespace XWave.Core.DTOs.Management;

public record StaffAccountDto
{
    public string StaffId { get; set; }
    public string FullName { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }

    public int RemaningDaysInContract
    {
        get
        {
            if (DateTime.Now > ContractEndDate) return 0;

            return (ContractEndDate - DateTime.Now).Days;
        }
    }

    public uint HourlyWage { get; set; }
    public string ImmediateManagerFullName { get; set; }
}