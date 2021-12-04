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

namespace XWave.Data.DatabaseSeeding
{
    public static class ProductSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                CreateCategories(context);
                CreateDiscounts(context);
                CreateProducts(context);
                context.Database.CloseConnection();
            }
        }

        public static void CreateCategories(ApplicationDbContext dbContext)
        {
            var categories = new List<Category>()
            {
                new Category()
                {
                    ID = 1,
                    Name = "GPU Cards",
                    Description = "Graphics processing units"
                },
                new Category()
                {
                    ID = 2,
                    Name = "Storages",
                    Description = "Hard drives, used to store data"
                },
                new Category()
                {
                    ID = 3,
                    Name = "Monitors",
                    Description = "Used to display the user interface"
                },
            };

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Categories ON") ;
            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Categories OFF") ;

        }
        public static void CreateProducts(ApplicationDbContext dbContext)
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
                    CategoryID = 2
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
                    CategoryID = 1
                },
            };
            dbContext.Products.AddRange(products);
            dbContext.SaveChanges();
        }
        public static void CreateDiscounts(ApplicationDbContext dbContext)
        {
            var discounts = new List<Discount>()
            {
                new Discount()
                {
                    ID = 1,
                    Percentage = 20,
                    StartDate = DateTime.ParseExact("7/1/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("11/2/2021", "d/M/yyyy", null)
                },
                new Discount()
                {
                    ID = 2,
                    Percentage = 35,
                    StartDate = DateTime.ParseExact("17/7/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("25/7/2021", "d/M/yyyy", null)
                }
            };
            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discounts ON") ;
            dbContext.Discounts.AddRange(discounts);
            dbContext.SaveChanges();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Discounts OFF") ;
        }
    }
}