using XWave.Data;

namespace XWave.Services.Defaults
{
    /// <summary>
    /// Abstract class that defines EF Core database context.
    /// </summary>
    public abstract class ServiceBase
    {
        protected XWaveDbContext DbContext { get; }

        public ServiceBase(XWaveDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}