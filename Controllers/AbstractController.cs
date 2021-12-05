using Microsoft.AspNetCore.Mvc;
using XWave.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace XWave.Controllers
{
    public class AbstractController<T> : ControllerBase where T : ControllerBase
    {
        protected XWaveDbContext DbContext { get; }
        protected ILogger<T> Logger { get; }
        public AbstractController(
            XWaveDbContext dbContext,
            ILogger<T> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }
    }
}
