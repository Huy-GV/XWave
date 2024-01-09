using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using XWave.Core.Data;
using XWave.Core.Services.Communication;

namespace XWave.UnitTest.Services;

/// <summary>
/// Sets up and manages a connection to the in-memory SQLite database.
/// </summary>
public abstract class BaseTest
{
    internal static async Task<XWaveDbContext> CreateDbContext()
    {
        var newConnection = new SqliteConnection("DataSource=:memory:;");
        await newConnection.OpenAsync();

        var dbContext = new XWaveDbContext(
            new DbContextOptionsBuilder<XWaveDbContext>()
                .UseSqlite(newConnection)
                .Options);

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    protected static void AssertEqualServiceResults<T>(ServiceResult<T> result1, ServiceResult<T> result2) where T : notnull
    {
        result1.Succeeded.Should().Be(result2.Succeeded);
        if (result1.Succeeded)
        {
            result1.Value.Should().BeEquivalentTo(result2.Value);
        }
        else
        {
            result1.Error.Should().BeEquivalentTo(result2.Error);
        }

    }

    protected static void AssertEqualServiceResults(ServiceResult result1, ServiceResult result2)
    {
        result1.Succeeded.Should().Be(result2.Succeeded);
        if (!result1.Succeeded)
        { 
            result1.Error.Should().BeEquivalentTo(result2.Error);
        }
    }
}