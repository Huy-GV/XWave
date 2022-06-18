namespace XWave.Core.DTOs.Management;

public record DetailedProductDto
{
    public int Id { get; init; }
    public string ProductName { get; init; }
    public string CategoryName { get; init; }
    public bool IsDiscontinued { get; init; } = false;
    public decimal Price { get; init; }
    public uint Quantity { get; init; }
    public DateTime? LatestRestock { get; set; }
    public int CategoryId { get; init; }
    public DetailedDiscountDto? Discount { get; init; }
}