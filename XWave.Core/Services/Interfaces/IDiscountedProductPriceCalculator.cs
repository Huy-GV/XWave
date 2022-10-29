using XWave.Core.Models;

namespace XWave.Core.Services.Interfaces
{
    internal interface IDiscountedProductPriceCalculator
    {
        /// <summary>
        ///     Calculate the discounted price of a product. Throws InvalidOperationException if product does not have any discount
        /// </summary>
        /// <param name="product">Product with a discount</param>
        /// <returns>Discounted price</returns>
        decimal CalculatePriceAfterDiscount(Product product);
    }
}