using FluentAssertions;
using FsCheck;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using XWave.Core.Data;
using XWave.Core.Services.Communication;

namespace XWave.UnitTest.Services;

/// <summary>
/// Sets up and manages a connection to the in-memory SQLite database.
/// </summary>
public class BaseTest : IDisposable
{
    protected DbConnection InMemoryDbConnection { get; }

    protected BaseTest()
    {
        InMemoryDbConnection = new SqliteConnection("DataSource=:memory:;");
        InMemoryDbConnection.Open();
    }

    internal XWaveDbContext CreateDbContext()
    {
        var dbContext = new XWaveDbContext(
            new DbContextOptionsBuilder<XWaveDbContext>()
            .UseSqlite(InMemoryDbConnection)
            .Options);

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        InMemoryDbConnection.Dispose();
    }

    protected void AssertEqualServiceResults<T>(ServiceResult<T> result1, ServiceResult<T> result2) where T : notnull
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

    protected void AssertEqualServiceResults(ServiceResult result1, ServiceResult result2)
    {
        result1.Succeeded.Should().Be(result2.Succeeded);
        if (!result1.Succeeded)
        { 
            result1.Error.Should().BeEquivalentTo(result2.Error);
        }
    }
}