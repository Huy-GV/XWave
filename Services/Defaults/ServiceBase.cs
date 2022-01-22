﻿using XWave.Data;

namespace XWave.Services.Defaults
{
    public abstract class ServiceBase
    {
        public XWaveDbContext DbContext { get; init; }
        public ServiceBase(XWaveDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}