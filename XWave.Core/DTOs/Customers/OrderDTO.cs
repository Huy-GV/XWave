namespace XWave.Core.DTOs.Customers;

public record OrderDto
{
    public IEnumerable<ProductPurchaseDetailsDto> ProductPurchaseDetails { get; set; }
    public DateTime OrderDate { get; set; }
    public string AccountNo { get; set; }
    public string Provider { get; set; }
    public int Id { get; set; }
}