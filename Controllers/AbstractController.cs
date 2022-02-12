using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
