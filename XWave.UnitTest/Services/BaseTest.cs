using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Services.Communication;

namespace XWave.UnitTest.Services;

/// <summary>
/// Sets up and manages a connection to the in-memory SQLite database.
/// </summary>
public class BaseTest
{
    internal static XWaveDbContext CreateDbContext()
    {
        var newConnection = new SqliteConnection("DataSource=:memory:;");
        newConnection.Open();

        var dbContext = new XWaveDbContext(
            new DbContextOptionsBuilder<XWaveDbContext>()
            .UseSqlite(newConnection)
            .Options);

        dbContext.Database.EnsureCreated();

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