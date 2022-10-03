namespace XWave.Core.DTOs.Customers;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public uint Quantity { get; init; }
    public int CategoryId { get; init; }
    public decimal? DiscountedPrice { get; init; } = null;
    public DiscountDto? Discount { get; init; } = null;
}