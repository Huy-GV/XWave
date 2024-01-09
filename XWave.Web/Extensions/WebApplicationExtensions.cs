using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using XWave.Core.Data.DatabaseSeeding.Seeders;

namespace XWave.Web.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await dataSeeder.SeedDataAsync();
    }
}
