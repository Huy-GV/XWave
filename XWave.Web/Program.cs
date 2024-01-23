using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using XWave.Core.Data.Constants;
using XWave.Core.Extension;
using XWave.Core.Options;
using XWave.Web.Data;
using XWave.Web.Extensions;
using XWave.Web.Middleware;
using XWave.Web.Options;
using XWave.Web.Utils;

namespace XWave.Web;

public class Program
{
    // well known environment variable set when the application is run in a container
    private const string DotnetRunningInContainerEnvVariable = "DOTNET_RUNNING_IN_CONTAINER";

    public static async Task Main(string[] args)
    {
        var builder = CreateApplicationBuilder(args);
        var app = builder.Build();

        await app.SeedDataAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
            app.UseHangfireDashboard();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "XWAVE.v1");
                options.RoutePrefix = "";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseMiddleware<RoleAuthorizationMiddleware>();
        app.UseAuthorization();
        app.MapControllerRoute(
                "default",
                "{controller}/{action=Index}/{id?}");
        
        await app.RunAsync();
    }

    private static WebApplicationBuilder CreateApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DotnetRunningInContainerEnvVariable)))
        {
            // some configurations such as JWT and database connection are configured via environment variables when deployed in Docker
            builder.Configuration.AddEnvironmentVariables();
        }

        builder.Services.AddDataSeeder(builder.Environment.EnvironmentName);

        // configure JWT
        builder.Services
            .AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services
            .AddOptions<JwtCookieOptions>()
            .BindConfiguration("JwtCookie")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        // configure core services and controllers
        builder.Services.AddTransient<IAuthenticationHelper, AuthenticationHelper>();
        builder.Services.AddControllers();
        builder.Services.AddCoreServices();

        // configure databases
        var dbConnectionString = GetDbConnectionString();
        builder.Services.AddDatabase(dbConnectionString);
        builder.Services.AddHangFireBackgroundServices(dbConnectionString);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookieName = builder.Configuration["JwtCookie:Name"]!;

                        // try populating token using the value from cookie
                        if (string.IsNullOrEmpty(context.Token) &&
                            context.Request.Cookies.ContainsKey(cookieName))
                        {
                            context.Token = context.Request.Cookies[cookieName];
                        }

                        return Task.CompletedTask;
                    }
                };

                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy(
                Policies.InternalPersonnelOnly, 
                policy => policy.RequireRole(RoleNames.Staff, RoleNames.Manager));

        builder.Services.AddScoped<RoleAuthorizationMiddleware>();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddSwaggerGen(config =>
        {
            config.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "XWave API",
                    Version = "v1",
                });
        });

        builder.Host
            .UseSerilog((_, services, configuration) => configuration
#if !DEBUG
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
#endif
            .ReadFrom.Services(services)
            .WriteTo.File(
                GetLogFileLocation(),
                shared: false,
                retainedFileCountLimit: 3,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 500 * 1000,
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console());

        return builder;

        string GetLogFileLocation()
        {
            var dockerEnv = Environment.GetEnvironmentVariable(DotnetRunningInContainerEnvVariable);
            var logDirectory = string.IsNullOrEmpty(dockerEnv)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XWave")
                : "log";
            var logFilePath = Path.Combine(logDirectory, "XWave.log");

            return logFilePath;
        }

        string GetDbConnectionString()
        {
            var (connectionKey, locationKey) = ("DefaultConnection", "DefaultDbLocation");
            var connection = builder.Configuration.GetConnectionString(connectionKey)!;
            var dbLocation = builder.Configuration.GetConnectionString(locationKey)!;

            if (string.IsNullOrEmpty(dbLocation))
            {
                return connection;
            }

            var dbDirectory = Path.GetDirectoryName(dbLocation)
                ?? throw new InvalidOperationException("Invalid database directory path");

            Console.WriteLine($"Creating database directory {dbDirectory}");
            Directory.CreateDirectory(dbDirectory);
            return $"{connection}AttachDbFileName={dbLocation};";
        }
    }
}
