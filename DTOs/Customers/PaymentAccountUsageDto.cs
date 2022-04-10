namespace XWave.DTOs.Customers
{
    public class PaymentAccountUsageDto
    {
        public string Provider { get; set; }
        public string AccountNumber { get; set; }
        public int TotalSpending { get; set; }
        public System.DateTime LatestPurchase { get; set; }
    }
}