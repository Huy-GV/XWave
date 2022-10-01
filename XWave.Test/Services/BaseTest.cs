using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using XWave.Core.Data;

namespace XWave.Test.Services;

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
}