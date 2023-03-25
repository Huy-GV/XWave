using System;
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
        var dockerEnv = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
        var logDirectory = string.IsNullOrEmpty(dockerEnv)
            ?  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XWave")
            : "log";
        var logFilePath = Path.Combine(logDirectory, "XWave.log");

        return Host.CreateDefaultBuilder(args)
            .UseSerilog((_, services, configuration) => configuration
#if !DEBUG
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
#endif
                .ReadFrom.Services(services)
                .WriteTo.File(
                    logFilePath,
                    shared: false,
                    retainedFileCountLimit: 3,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 500 * 1000,
                    outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console())
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}