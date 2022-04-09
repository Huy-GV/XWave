using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using XWave.Data.Constants;
using XWave.Controllers;
using System.Linq;

namespace XWave.Extensions
{
    public static class ControllerExtension
    {
        public static ActionResult XWaveCreated(this ControllerBase controller, string url)
        {
            return controller.StatusCode(201, new { url });
        }

        public static ActionResult XWaveBadRequest(this ControllerBase controller, IEnumerable<string> errors)
        {
            return controller.BadRequest(XWaveResponse.Failed(errors.ToArray()));
        }

        public static ActionResult XWaveBadRequest(this ControllerBase controller, params string[] errors)
        {
            return controller.BadRequest(XWaveResponse.Failed(errors));
        }

        public static ActionResult XWaveUpdated(this ControllerBase controller, string url)
        {
            return controller.Ok(url);
        }
    }
}