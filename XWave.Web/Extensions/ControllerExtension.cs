using System.Net;
using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ControllerExtension
{
    public static ActionResult Created(this ControllerBase controller, string url)
    {
        return controller.StatusCode((int)HttpStatusCode.Created, new { url });
    }

    public static ActionResult Updated(this ControllerBase controller, string url)
    {
        return controller.Ok(url);
    }

    public static ActionResult MapErrorCodeToHttpCode(this ControllerBase controller, Error error)
    {
        return error.Code switch 
        {
            ErrorCode.EntityNotFound => controller.NotFound(),
            ErrorCode.AuthenticationError => controller.Unauthorized(),
            ErrorCode.AuthorizationError => controller.Forbid(),
            ErrorCode.ConflictingState => controller.Conflict(error),
            _ => controller.BadRequest(error)
        };
    }
}