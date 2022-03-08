using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XWave.Data.Constants;

namespace XWave.Controllers
{
    public class AbstractController<T> : ControllerBase where T : ControllerBase
    {
        protected ILogger<T> Logger { get; }
        public AbstractController(ILogger<T> logger)
        {
            Logger = logger;
        }
        public ActionResult XWaveCreated(string url)
        {
            return Ok(XWaveResponse.Created(url));
        }
        public ActionResult XWaveBadRequest(string error)
        {
            return BadRequest(XWaveResponse.Failed(error));
        }
        public ActionResult XWaveUpdated(string url)
        {
            return Ok(XWaveResponse.Updated(url));
        }
    }
}
