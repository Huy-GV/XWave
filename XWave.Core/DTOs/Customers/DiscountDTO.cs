namespace XWave.Core.DTOs.Customers;

public record DiscountDto
{
    public uint Percentage { get; init; }
    public DateTime EndDate { get; init; }
}