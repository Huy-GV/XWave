using System;

namespace XWave.DTOs
{
    public class ProductDTO
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public int CategoryID { get; init; }
        public DiscountDTO? Discount { get; init; }
    }
}
