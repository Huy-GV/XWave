using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace XWave.IntegrationTest.Factories;

public class XWaveApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DatabaseName = "XWaveTest";
    private const string Username = "sa";
    private const ushort MsSqlPort = 1433;

    // use random host port so multiple containers can run at the same time
    private const bool UseRandomHostPort = true;

    private readonly IContainer _mssqlContainer;
    private readonly string _databasePassword;

    public XWaveApiWebApplicationFactory()
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        _databasePassword = configuration["SeedData:Password"];

        _mssqlContainer = new ContainerBuilder()
            .WithName($"XWaveTestcontainer_{Guid.NewGuid()}")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MsSqlPort, UseRandomHostPort)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", _databasePassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var host = _mssqlContainer.Hostname;
        var port = _mssqlContainer.GetMappedPublicPort(MsSqlPort);

        builder.ConfigureServices(services =>
        {
            services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(DbContextOptions<XWaveDbContext>)));

            services.AddDbContext<XWaveDbContext>(options =>
                options.UseSqlServer($"Server={host},{port};Database={DatabaseName};User Id={Username};Password={_databasePassword};TrustServerCertificate=True"));
        });

    }
}
