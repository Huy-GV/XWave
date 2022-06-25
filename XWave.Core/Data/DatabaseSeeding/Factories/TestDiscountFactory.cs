using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories
{
    public class TestDiscountFactory
    {
        public static List<Discount> Discounts()
        {
            return new List<Discount>
            {
                new()
                {
                    Id = 1,
                    Percentage = 20,
                    StartDate = DateTime.ParseExact("7/1/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("11/2/2021", "d/M/yyyy", null)
                },
                new()
                {
                    Id = 2,
                    Percentage = 35,
                    StartDate = DateTime.ParseExact("17/7/2021", "d/M/yyyy", null),
                    EndDate = DateTime.ParseExact("25/7/2021", "d/M/yyyy", null)
                }
            };
        }
    }
}