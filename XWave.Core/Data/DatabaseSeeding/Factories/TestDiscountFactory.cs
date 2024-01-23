using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;

public static class TestDiscountFactory
{
    public static List<Discount> Discounts()
    {
        return
        [
            new Discount
            {
                Percentage = 20,
                StartDate = DateTime.ParseExact("7/1/2021", "d/M/yyyy", null),
                EndDate = DateTime.ParseExact("11/2/2021", "d/M/yyyy", null)
            },

            new Discount
            {
                Percentage = 35,
                StartDate = DateTime.ParseExact("17/7/2021", "d/M/yyyy", null),
                EndDate = DateTime.ParseExact("25/7/2021", "d/M/yyyy", null)
            }
        ];
    }
}