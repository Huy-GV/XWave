using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;

public class TestProductFactory
{
    public static List<Product> Products(List<Category> categories, List<Discount> discounts)
    {
        // todo: use indexes instead?
        var randomIndex = new Random();
        var minCategoryPK = categories.Select(x => x.Id).Min();
        var minDiscountPK = discounts.Select(x => x.Id).Min();
        var products = new List<Product>
        {
            new()
            {
                Name = "HD Pro Monitor",
                Description = "Monitor suitable for daily tasks",
                Price = 200,
                Quantity = 150,
                LatestRestock = DateTime.ParseExact("2/2/2020", "d/M/yyyy", null),
                CategoryId = randomIndex.Next(minCategoryPK, categories.Count),
                IsDiscontinued = true,
                DiscontinuationDate = GenerateRandomPastDate(randomIndex)
            },
            new ()
            {
                Name = "ROG RAM Card",
                Description = "Monitor suitable for daily tasks",
                Price = 80,
                Quantity = 70,
                LatestRestock = DateTime.ParseExact("14/9/2021", "d/M/yyyy", null),
                CategoryId = randomIndex.Next(minCategoryPK, categories.Count),
            },

            new ()
            {
                Name = "Corsair SSD 4GB",
                Description = "Monitor suitable for daily tasks",
                Price = 160,
                Quantity = 200,
                LatestRestock = DateTime.ParseExact("3/12/2021", "d/M/yyyy", null),
                CategoryId = randomIndex.Next(minCategoryPK, categories.Count),
                DiscountId = randomIndex.Next(minDiscountPK, discounts.Count),
                IsDiscontinued = true,
                DiscontinuationDate = GenerateRandomPastDate(randomIndex)
            },

            new()
            {
                Name = "NVIDIA GTX 3080",
                Description = "Monitor suitable for daily tasks",
                Price = 1800,
                Quantity = 10,
                LatestRestock = DateTime.ParseExact("11/10/2021", "d/M/yyyy", null),
                CategoryId = randomIndex.Next(minCategoryPK, categories.Count),
                DiscountId = randomIndex.Next(minDiscountPK, discounts.Count),
            }
        };

        return products;
    }
    private static DateTime GenerateRandomPastDate(Random random)
    {
        int daysInPast = random.Next(1, 366);
        return DateTime.Now.AddDays(-daysInPast);
    }
}