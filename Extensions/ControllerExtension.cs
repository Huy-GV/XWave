using System.Net;
using Microsoft.AspNetCore.Mvc;
using XWave.Data.Constants;

namespace XWave.Extensions;

public static class ControllerExtension
{
    public static ActionResult XWaveCreated(this ControllerBase controller, string url)
    {
        return controller.StatusCode((int)HttpStatusCode.Created, new { url });
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