using Microsoft.AspNetCore.Mvc;
using XWave.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Controllers
{
    public class AbstractController<T> : ControllerBase where T : ControllerBase
    {
        protected ILogger<T> Logger { get; }
        public AbstractController(ILogger<T> logger)
        {
            Logger = logger;
        }
    }
}
