namespace XWave.Core.DTOs.Customers;

public record ProductPurchaseDetailsDto
{
    public uint Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal Total => Quantity * Price;
    public string ProductName { get; init; } = string.Empty;
    //public string ProductImage { get; set; }
}