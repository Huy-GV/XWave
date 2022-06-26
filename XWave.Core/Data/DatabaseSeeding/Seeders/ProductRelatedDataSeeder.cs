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
        var tableNames = new[] { "Category", "Product", "Discount" }.Select(x => $"dbo.{x}");

        try
        {
            context.Database.OpenConnection();
            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT ${tableName} ON");
            }

            CreateCategories(context);
            CreateDiscounts(context);
            CreateProducts(context);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ProductRelatedDataSeeder>>();
            logger.LogError(ex, "An error occurred while seeding product data");
            logger.LogError(ex.Message);
        }
        finally
        {
            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT ${tableName} OFF");
            }
            context.Database.CloseConnection();
        }
    }

    public static void CreateCategories(XWaveDbContext dbContext)
    {
        dbContext.Category.AddRange(TestCategoryFactory.Categories());
        dbContext.SaveChanges();
    }

    public static void CreateProducts(XWaveDbContext dbContext)
    {
        dbContext.Product.AddRange(TestProductFactory.Products());
        dbContext.SaveChanges();
    }

    public static void CreateDiscounts(XWaveDbContext dbContext)
    {
        dbContext.Discount.AddRange(TestDiscountFactory.Discounts());
        dbContext.SaveChanges();
    }
}