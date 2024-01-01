using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using XWave.Core.Configuration;
using XWave.Core.Data.Constants;
using XWave.Core.Extension;
using XWave.Web.Data;
using XWave.Web.Middleware;
using XWave.Web.Utils;

namespace XWave.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            await app.Services.SeedDevelopmentDatabaseAsync();
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
            await app.Services.SeedProductionDatabaseAsync();
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
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller}/{action=Index}/{id?}");
        });

        await app.RunAsync();
    }

    private static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<Jwt>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<JwtCookie>(builder.Configuration.GetSection("JwtCookie"));

        builder.Services.AddTransient<AuthenticationHelper>();
        builder.Services.AddControllers();
        builder.Services.AddDefaultXWaveServices();

        var dbConnectionString = GetDbConnectionString();
        Console.WriteLine($"\n{dbConnectionString}\n");
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
                        var cookieName = builder.Configuration["JwtCookie:Name"];
                        // accommodate clients without cookies
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
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.InternalPersonnelOnly,
                policy => policy.RequireRole(RoleNames.Staff, RoleNames.Manager));
        });

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
            var dockerEnv = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
            var logDirectory = string.IsNullOrEmpty(dockerEnv)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XWave")
                : "log";
            var logFilePath = Path.Combine(logDirectory, "XWave.log");

            return logFilePath;
        }

        string GetDbConnectionString()
        {
            var dockerEnv = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
            var (connectionKey, locationKey) = string.IsNullOrEmpty(dockerEnv)
                ? ("DefaultConnection", "DefaultDbLocation")
                : ("ContainerConnection", "ContainerDbLocation");

            var connection = builder.Configuration.GetConnectionString(connectionKey);
            var dbLocation = builder.Configuration.GetConnectionString(locationKey);

            if (string.IsNullOrEmpty(dbLocation))
            {
                return connection;
            }

            var dbDirectory = Path.GetDirectoryName(dbLocation)
                ?? throw new InvalidOperationException("Invalid database directory path");
            Directory.CreateDirectory(dbDirectory);
            return $"{connection}AttachDbFileName={dbLocation};";
        }
    }
}
