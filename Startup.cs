using System;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using XWave.Configuration;
using XWave.Data;
using XWave.Data.Constants;
using XWave.Extensions;
using XWave.Middleware;
using XWave.Models;
using XWave.Services.Defaults;
using XWave.Services.Interfaces;

namespace XWave;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<Jwt>(Configuration.GetSection("Jwt"));
        services.Configure<JwtCookie>(Configuration.GetSection("JwtCookie"));

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options => { options.Lockout.AllowedForNewUsers = false; })
            .AddEntityFrameworkStores<XWaveDbContext>();

        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddControllers();
        services.AddDefaultXWaveServices();
        services.AddDefaultHelpers();

        services.AddHangFireBackgroundServices(Configuration.GetConnectionString("DefaultConnection"));

        services
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
                        var cookieName = Configuration["JwtCookie:Name"];
                        // accommodate clients without cookies
                        if (string.IsNullOrEmpty(context.Token) &&
                            context.Request.Cookies.ContainsKey(cookieName))
                            context.Token = context.Request.Cookies[cookieName];

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
                    ValidIssuer = Configuration["JWT:Issuer"],
                    ValidAudience = Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["JWT:Key"]))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.InternalPersonnelOnly,
                policy => policy.RequireRole(Roles.Staff, Roles.Manager));
        });

        services.AddScoped<RoleAuthorizationMiddleware>();

        services.AddDbContext<XWaveDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddDatabaseDeveloperPageExceptionFilter();

        // In production, the React files will be served from this directory
        //services.AddSpaStaticFiles(configuration =>
        //{
        //    configuration.RootPath = "ClientApp/build";
        //});
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
            app.UseHangfireDashboard();
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
        //app.UseSpaStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        //app.UseIdentityServer();
        app.UseMiddleware<RoleAuthorizationMiddleware>();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller}/{action=Index}/{id?}");
        });

        //app.UseSpa(spa =>
        //{
        //    spa.Options.SourcePath = "ClientApp";

        //    if (env.IsDevelopment())
        //    {
        //        spa.UseReactDevelopmentServer(npmScript: "start");
        //    }
        //});
    }
}