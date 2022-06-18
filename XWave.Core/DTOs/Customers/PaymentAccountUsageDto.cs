namespace XWave.Core.DTOs.Customers;

public record PaymentAccountUsageDto
{
    public string Provider { get; init; }
    public string AccountNumber { get; init; }
    public decimal TotalSpending { get; init; }
    public ushort PurchaseCount { get; init; }
    public DateTime LatestPurchase { get; init; }
}