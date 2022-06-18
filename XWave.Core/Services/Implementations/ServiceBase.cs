using XWave.Core.Data;

namespace XWave.Core.Services.Implementations;

/// <summary>
///     Abstract class that defines EF Core database context.
/// </summary>
internal abstract class ServiceBase
{
    internal ServiceBase(XWaveDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected XWaveDbContext DbContext { get; }
}