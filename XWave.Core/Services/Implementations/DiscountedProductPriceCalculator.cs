using XWave.Core.Models;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations
{
    public class DiscountedProductPriceCalculator : IDiscountedProductPriceCalculator
    {
        public decimal CalculatePriceAfterDiscount(Product product)
        {
            if (product.Discount is null) 
            {
                throw new InvalidOperationException($"Product ID {product.Id} does not have any discount");
            }
            
            return product.Price - product.Price * product.Discount.Percentage / 100;
        }
    }
}