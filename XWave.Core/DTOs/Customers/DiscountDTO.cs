namespace XWave.Core.DTOs.Customers;

public record DiscountDto
{
    public uint Percentage { get; set; }
    public DateTime EndDate { get; set; }
}