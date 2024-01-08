using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;
internal class ProductionDataSeeder : IDataSeeder
{
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly XWaveDbContext _dbContext;

    public ProductionDataSeeder(
        XWaveDbContext dbContext,
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _configuration = configuration;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SeedDataAsync()
    {
        await _dbContext.Database.MigrateAsync();

        try
        {
            await UserSeeder.SeedProductionDataAsync(_dbContext, _configuration, _roleManager, _userManager, _logger);
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while seeding production data");
            throw;
        }
    }
}
