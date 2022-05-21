using XWave.Data;

namespace XWave.Services.Defaults;

/// <summary>
///     Abstract class that defines EF Core database context.
/// </summary>
public abstract class ServiceBase
{
    public ServiceBase(XWaveDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected XWaveDbContext DbContext { get; }
}