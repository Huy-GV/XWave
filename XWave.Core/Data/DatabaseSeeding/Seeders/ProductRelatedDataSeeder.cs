using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class ProductRelatedDataSeeder
{
    public static async Task SeedData<TSeeder>(
        XWaveDbContext context, 
        UserManager<ApplicationUser> userManager, 
        ILogger<TSeeder> logger) where TSeeder : IDataSeeder
    {
        try
        {
            var categories = await CreateCategoriesAsync(context);
            var discounts = await CreateDiscountsAsync(context);
            await CreateProductsAsync(context, categories, discounts);
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while seeding product-related data");
            throw;
        }
    }

    private static async Task<List<Category>> CreateCategoriesAsync(XWaveDbContext dbContext)
    {
        var categories = TestCategoryFactory.Categories();
        dbContext.Category.AddRange(categories);
        await dbContext.SaveChangesAsync();
        return categories;
    }

    private static async Task<List<Product>> CreateProductsAsync(
        XWaveDbContext dbContext,
        List<Category> categories,
        List<Discount> discounts)
    {
        var products = TestProductFactory.Products(categories, discounts);
        dbContext.Product.AddRange(products);
        await dbContext.SaveChangesAsync();
        return products;
    }

    private static async Task<List<Discount>> CreateDiscountsAsync(XWaveDbContext dbContext)
    {
        var discounts = TestDiscountFactory.Discounts();
        dbContext.Discount.AddRange(discounts);
        await dbContext.SaveChangesAsync(); 
        return discounts;
    }
}