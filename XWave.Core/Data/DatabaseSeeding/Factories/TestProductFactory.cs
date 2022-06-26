﻿using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories
{
    public class TestProductFactory
    {
        // todo: pass categories and discount here to prevent FK errors

        public static List<Product> Products()
        {
            return new List<Product>
            {
                new()
                {
                    Name = "HD Pro Monitor",
                    Description = "Monitor suitable for daily tasks",
                    Price = 200,
                    Quantity = 150,
                    LatestRestock = DateTime.Parse("2/2/2020"),
                    CategoryId = 3
                },
                new()
                {
                    Name = "ROG RAM Card",
                    Description = "Monitor suitable for daily tasks",
                    Price = 80,
                    Quantity = 70,
                    LatestRestock = DateTime.Parse("14/9/2021"),
                    CategoryId = 2,
                    DiscountId = 2
                },
                new()
                {
                    Name = "Corsair SSD 4GB",
                    Description = "Monitor suitable for daily tasks",
                    Price = 160,
                    Quantity = 200,
                    LatestRestock = DateTime.Parse("3/12/2021"),
                    CategoryId = 2,
                    DiscountId = 1
                },
                new()
                {
                    Name = "NVIDIA GTX 3080",
                    Description = "Monitor suitable for daily tasks",
                    Price = 1800,
                    Quantity = 10,
                    LatestRestock = DateTime.Parse("11/10/2021"),
                    CategoryId = 1,
                    DiscountId = 2
                }
            };
        }
    }
}