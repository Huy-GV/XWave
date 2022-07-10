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
                    Name = "GPU Cards",
                    Description = "Graphics processing units"
                },
                new()
                {
                    Name = "Storages",
                    Description = "Hard drives, used to store data"
                },
                new()
                {
                    Name = "Monitors",
                    Description = "Used to display the user interface"
                }
            };
        }
    }
}