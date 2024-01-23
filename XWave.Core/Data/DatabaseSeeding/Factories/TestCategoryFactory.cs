using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;

public static class TestCategoryFactory
{
    public static List<Category> Categories()
    {
        return
        [
            new Category
            {
                Name = "GPU Cards",
                Description = "Graphics processing units"
            },

            new Category
            {
                Name = "Storages",
                Description = "Hard drives, used to store data"
            },

            new Category
            {
                Name = "Monitors",
                Description = "Used to display the user interface"
            }
        ];
    }
}