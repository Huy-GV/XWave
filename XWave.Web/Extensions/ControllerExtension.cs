using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace XWave.Web.Extensions;

public static class ControllerExtension
{
    public static ActionResult Created(this ControllerBase controller, string url)
    {
        return controller.StatusCode((int)HttpStatusCode.Created, new { url });
    }

    public static ActionResult Updated(this ControllerBase controller, string url)
    {
        return controller.Ok(new { url });
    }

    public static string ApiUrl(this ControllerBase controller) 
    {
        return $"{controller.Request.Scheme}://{controller.Request.Host.Value}/api";
    }
}