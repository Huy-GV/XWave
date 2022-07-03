using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;

internal class ProductRelatedDataSeeder
{
    private static string SetIdentityInsertQuery(bool status, string tableName) => $"SET IDENTITY_INSERT dbo.{tableName} {(status ? "ON" : "OFF")}";

    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var context = new XWaveDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<XWaveDbContext>>());

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var tableNames = new[] { "Category", "Product", "Discount" }.Select(x => $"dbo.{x}");

        try
        {
            context.Database.OpenConnection();
            var categories = CreateCategories(context);
            var discounts = CreateDiscounts(context);
            CreateProducts(context, categories, discounts);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ProductRelatedDataSeeder>>();
            logger.LogError("An error occurred while seeding product related data");
            logger.LogDebug(ex.Message, ex);
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }

    public static List<Category> CreateCategories(XWaveDbContext dbContext)
    {
        try
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(true, "Category"));

            var categories = TestCategoryFactory.Categories();
            dbContext.Category.AddRange(categories);
            dbContext.SaveChanges();
            return categories;
        }
        finally
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(false, "Category"));
        }
    }

    public static List<Product> CreateProducts(
        XWaveDbContext dbContext,
        List<Category> categories,
        List<Discount> discounts)
    {
        try
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(true, "Product"));
            var products = TestProductFactory.Products(categories, discounts);
            dbContext.Product.AddRange(products);
            dbContext.SaveChanges();
            return products;
        }
        finally
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(false, "Product"));
        }
    }

    public static List<Discount> CreateDiscounts(XWaveDbContext dbContext)
    {
        try
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(true, "Discount"));

            var discounts = TestDiscountFactory.Discounts();
            dbContext.Discount.AddRange(discounts);
            dbContext.SaveChanges();
            return discounts;
        }
        finally
        {
            dbContext.Database.ExecuteSqlRaw(SetIdentityInsertQuery(false, "Discount"));
        }
    }
}