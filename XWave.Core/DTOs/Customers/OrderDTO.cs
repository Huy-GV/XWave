namespace XWave.Core.DTOs.Customers;

public record OrderDto
{
    public IEnumerable<ProductPurchaseDetailsDto> ProductPurchaseDetails { get; set; } = Enumerable.Empty<ProductPurchaseDetailsDto>();
    public DateTime OrderDate { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public int Id { get; set; }
}