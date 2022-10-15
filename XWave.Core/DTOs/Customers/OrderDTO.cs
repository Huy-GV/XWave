namespace XWave.Core.DTOs.Customers;

public record OrderDto
{
    public IEnumerable<ProductPurchaseDetailsDto> ProductPurchaseDetails { get; init; } = Enumerable.Empty<ProductPurchaseDetailsDto>();
    public DateTime OrderDate { get; init; }
    public string AccountNo { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public int Id { get; init; }
}