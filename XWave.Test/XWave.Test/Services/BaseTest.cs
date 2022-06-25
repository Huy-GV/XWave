using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWave.Core.Data;

namespace XWave.Test.Services
{
    /// <summary>
    /// Sets up and manages a connection to the in-memory SQLite database.
    /// </summary>
    public class BaseTest : IDisposable
    {
        protected DbConnection InMemoryDbConnection { get; }

        protected BaseTest()
        {
            InMemoryDbConnection = new SqliteConnection("Filename=:memory:");
            InMemoryDbConnection.Open();
        }

        internal XWaveDbContext CreateDbContext()
        {
            return new XWaveDbContext(
                new DbContextOptionsBuilder<XWaveDbContext>()
                .UseSqlite(InMemoryDbConnection)
                .Options);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            InMemoryDbConnection.Dispose();
        }
    }
}