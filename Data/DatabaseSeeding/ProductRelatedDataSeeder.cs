using System.Threading.Tasks;
using System;
using XWave.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XWave.Data;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.Extensions.Options;
using XWave.Data.Constants;
using System.Collections.Generic;
using System.Linq;
using XWave.Models;
using Microsoft.Extensions.Logging;

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
            } catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding product data");
                logger.LogError(ex.Message);
            } finally
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
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Category ON") ;
            dbContext.Category.AddRange(categories);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Category OFF") ;

        }
        public static void CreateProducts(XWaveDbContext dbContext)
        {
            var products = new List<Product>()
            {
                new Product() 
                {
                    Name = "HD Pro Monitor",
                    Price = 200,
                    Quantity = 150,
                    LastRestock = DateTime.Parse("2/2/2020"),
                    CategoryID = 3
                },
                new Product() 
                {
                    Name = "ROG RAM Card",
                    Price = 80,
                    Quantity = 70,
                    LastRestock = DateTime.Parse("14/9/2021"),
                    CategoryID = 2,
                    DiscountID = 2
                },
                new Product() 
                {
                    Name = "Corsair SSD 4GB",
                    Price = 160,
                    Quantity = 200,
                    LastRestock = DateTime.Parse("3/12/2021"),
                    CategoryID = 2,
                    DiscountID = 1
                },
                new Product() 
                {
                    Name = "NVIDIA GTX 3080",
                    Price = 1800,
                    Quantity = 10,
                    LastRestock = DateTime.Parse("11/10/2021"),
                    CategoryID = 1,
                    DiscountID = 2
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
                    ManagerId = managers[0].Id,
                },
                new Discount()
                {
                    Id = 2,
                    Percentage = 35,
                    StartDate = DateTime.ParseExact("17/7/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("25/7/2021", "d/M/yyyy", null),
                    ManagerId = managers[1].Id,
                }
            };
            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discount ON") ;
            dbContext.Discount.AddRange(discounts);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discount OFF") ;
        }
    }
}