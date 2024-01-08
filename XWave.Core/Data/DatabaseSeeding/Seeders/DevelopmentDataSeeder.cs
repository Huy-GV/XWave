using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Seeders;
internal class DevelopmentDataSeeder : IDataSeeder
{
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly XWaveDbContext _dbContext;

    public DevelopmentDataSeeder(
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
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();

        try
        {
            await UserSeeder.SeedDevelopmentDataAsync(_dbContext, _configuration, _roleManager, _userManager, _logger);
            await ProductRelatedDataSeeder.SeedData(_dbContext, _userManager, _logger);
            await PurchaseRelatedDataSeeder.SeedData(_dbContext, _userManager, _logger);
            await StaffActivitySeeder.SeedData(_dbContext, _userManager, _logger);
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while seeding development data");
            throw;
        }
    }
}
