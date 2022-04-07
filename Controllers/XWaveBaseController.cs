using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using XWave.Data.Constants;

namespace XWave.Controllers
{
    public class XWaveBaseController : ControllerBase
    {
        // todo: move to extension methods
        public ActionResult XWaveCreated(string url)
        {
            return Ok(XWaveResponse.Created(url));
        }

        public ActionResult XWaveBadRequest(string error)
        {
            return BadRequest(XWaveResponse.Failed(error));
        }

        public ActionResult XWaveBadRequest(IEnumerable<string> errors)
        {
            return BadRequest(new { Errors = errors });
        }

        public ActionResult XWaveUpdated(string url)
        {
            return Ok(XWaveResponse.Updated(url));
        }
    }
}