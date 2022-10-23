using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class ProductRelatedDataSeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<XWaveDbContext>>());

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        try
        {
            var categories = CreateCategories(context);
            var discounts = CreateDiscounts(context);
            CreateProducts(context, categories, discounts);
        }
        catch (Exception)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ProductRelatedDataSeeder>>();
            logger.LogError("An error occurred while seeding product related data");
            throw;
        }
    }

    private static List<Category> CreateCategories(XWaveDbContext dbContext)
    {
        var categories = TestCategoryFactory.Categories();
        dbContext.Category.AddRange(categories);
        dbContext.SaveChanges();
        return categories;
    }

    private static List<Product> CreateProducts(
        XWaveDbContext dbContext,
        List<Category> categories,
        List<Discount> discounts)
    {
        var products = TestProductFactory.Products(categories, discounts);
        dbContext.Product.AddRange(products);
        dbContext.SaveChanges();
        return products;
    }

    private static List<Discount> CreateDiscounts(XWaveDbContext dbContext)
    {
        var discounts = TestDiscountFactory.Discounts();
        dbContext.Discount.AddRange(discounts);
        dbContext.SaveChanges();
        return discounts;
    }
}