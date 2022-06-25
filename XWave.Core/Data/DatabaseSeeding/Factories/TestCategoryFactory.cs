using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories
{
    public class TestCategoryFactory
    {
        public static List<Category> Categories()
        {
            return new List<Category>
            {
                new()
                {
                    Id = 1,
                    Name = "GPU Cards",
                    Description = "Graphics processing units"
                },
                new()
                {
                    Id = 2,
                    Name = "Storages",
                    Description = "Hard drives, used to store data"
                },
                new()
                {
                    Id = 3,
                    Name = "Monitors",
                    Description = "Used to display the user interface"
                }
            };
        }
    }
}