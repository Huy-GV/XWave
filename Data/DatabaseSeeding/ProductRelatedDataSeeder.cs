using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using XWave.Data.Constants;
using XWave.Models;

namespace XWave.Data.DatabaseSeeding
{
    public static class ProductRelatedDataSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using var context = new XWaveDbContext(
                serviceProvider
                .GetRequiredService<DbContextOptions<XWaveDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            try
            {
                CreateCategories(context);
                CreateDiscounts(context, userManager);
                CreateProducts(context);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding product data");
                logger.LogError(ex.Message);
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        public static void CreateCategories(XWaveDbContext dbContext)
        {
            var categories = new List<Category>()
            {
                new Category()
                {
                    Id = 1,
                    Name = "GPU Cards",
                    Description = "Graphics processing units"
                },
                new Category()
                {
                    Id = 2,
                    Name = "Storages",
                    Description = "Hard drives, used to store data"
                },
                new Category()
                {
                    Id = 3,
                    Name = "Monitors",
                    Description = "Used to display the user interface"
                },
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Category ON");
            dbContext.Category.AddRange(categories);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Category OFF");
        }

        public static void CreateProducts(XWaveDbContext dbContext)
        {
            var products = new List<Product>()
            {
                new Product()
                {
                    Name = "HD Pro Monitor",
                    Description = "Monitor suitable for daily tasks",
                    Price = 200,
                    Quantity = 150,
                    LatestRestock = DateTime.Parse("2/2/2020"),
                    CategoryId = 3
                },
                new Product()
                {
                    Name = "ROG RAM Card",
                    Description = "Monitor suitable for daily tasks",
                    Price = 80,
                    Quantity = 70,
                    LatestRestock = DateTime.Parse("14/9/2021"),
                    CategoryId = 2,
                    DiscountId = 2
                },
                new Product()
                {
                    Name = "Corsair SSD 4GB",
                    Description = "Monitor suitable for daily tasks",
                    Price = 160,
                    Quantity = 200,
                    LatestRestock = DateTime.Parse("3/12/2021"),
                    CategoryId = 2,
                    DiscountId = 1
                },
                new Product()
                {
                    Name = "NVIDIA GTX 3080",
                    Description = "Monitor suitable for daily tasks",
                    Price = 1800,
                    Quantity = 10,
                    LatestRestock = DateTime.Parse("11/10/2021"),
                    CategoryId = 1,
                    DiscountId = 2
                },
            };

            dbContext.Product.AddRange(products);
            dbContext.SaveChanges();
        }

        public static void CreateDiscounts(
            XWaveDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            var managers = userManager.GetUsersInRoleAsync(Roles.Manager).Result;
            var discounts = new List<Discount>()
            {
                new Discount()
                {
                    Id = 1,
                    Percentage = 20,
                    StartDate = DateTime.ParseExact("7/1/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("11/2/2021", "d/M/yyyy", null),
                },
                new Discount()
                {
                    Id = 2,
                    Percentage = 35,
                    StartDate = DateTime.ParseExact("17/7/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("25/7/2021", "d/M/yyyy", null),
                }
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discount ON");
            dbContext.Discount.AddRange(discounts);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discount OFF");
        }
    }
}