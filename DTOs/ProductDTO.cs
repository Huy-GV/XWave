namespace XWave.DTOs
{
    public class ProductDTO
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public int Quantity { get; init; }
        public int? CategoryID { get; init; }
        public int? DiscountID { get; init; }
        public static ProductDTO From(Models.Product product)
        {
            return new ProductDTO()
            {
                ID = product.ID,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryID = product.CategoryID,
                DiscountID = product.DiscountID,
            };
        }
    }
}
