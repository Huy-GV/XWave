using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using XWave.Core.Extension;

namespace XWave.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.SeedDatabase();

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((_, services, configuration) => configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "XWave.Log"),
                    shared: true)
                .WriteTo.Console())
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}