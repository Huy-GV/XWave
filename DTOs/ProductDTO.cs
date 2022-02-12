namespace XWave.DTOs
{
    //TODO: remove all methods from DTO and use a mapper library
    public class ProductDTO
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public int? CategoryID { get; init; }
        public DiscountDTO? DiscountDTO { get; init; }
        public static ProductDTO From(Models.Product product)
        {
            DiscountDTO discountDTO = null;
            if (product.Discount != null)
                discountDTO = DiscountDTO.From(product.Discount);

            return new ProductDTO()
            {
                ID = product.ID,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.Name ?? string.Empty,
                DiscountDTO = discountDTO
            };
        }
    }
}
