namespace XWave.DTOs.Customers
{
    public record ProductDto
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public int CategoryId { get; init; }
        public DiscountDto? Discount { get; init; }
    }
}