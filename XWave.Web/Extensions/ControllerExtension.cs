using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace XWave.Web.Extensions;

public static class ControllerExtension
{
    public static ActionResult XWaveCreated(this ControllerBase controller, string url)
    {
        return controller.StatusCode((int)HttpStatusCode.Created, new { url });
    }

    public static ActionResult XWaveUpdated(this ControllerBase controller, string url)
    {
        return controller.Ok(url);
    }
}