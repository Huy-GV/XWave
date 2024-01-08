
namespace XWave.Core.Data.DatabaseSeeding.Seeders;

public interface IDataSeeder
{
    /// <summary>
    /// Seed required data for the application.
    /// </summary>
    /// <returns></returns>
    Task SeedDataAsync();
}