using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using XWave.Core.Data;
using XWave.IntegrationTest.Factories;

namespace XWave.IntegrationTest.Endpoints;
public abstract class BaseTest : IClassFixture<XWaveApiWebApplicationFactory>
{
    public BaseTest(XWaveApiWebApplicationFactory webApplicationFactory)
    {
        XWaveApplicationFactory = webApplicationFactory;
    }

    protected XWaveApiWebApplicationFactory XWaveApplicationFactory { get; }

    internal AsyncServiceScope CreateScope()
    {
        return XWaveApplicationFactory.Services.CreateAsyncScope();
    }

    internal static XWaveDbContext CreateDbContext(IServiceScope scope)
    {
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<XWaveDbContext>>();

        return new XWaveDbContext(options);
    }
}
