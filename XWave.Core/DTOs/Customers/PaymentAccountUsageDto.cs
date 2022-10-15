namespace XWave.Core.DTOs.Customers;

public record PaymentAccountUsageDto
{
    public string Provider { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public decimal TotalSpending { get; init; }
    public int PurchaseCount { get; init; }
    public DateTime? LatestPurchase { get; init; }
}