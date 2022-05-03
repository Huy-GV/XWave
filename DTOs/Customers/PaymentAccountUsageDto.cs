namespace XWave.DTOs.Customers
{
    public record PaymentAccountUsageDto()
    {
        public string Provider { get; init; }
        public string AccountNumber { get; init; }
        public decimal TotalSpending { get; init; }
        public ushort PurchaseCount { get; init; }
        public System.DateTime LatestPurchase { get; init; }
    }
}