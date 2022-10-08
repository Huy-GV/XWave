using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ResultMapper
{
    public static ActionResult MapErrorToHttpCode(Error error)
    {
        return error.Code switch 
        {
            ErrorCode.EntityNotFound => new NotFoundResult(),
            ErrorCode.AuthenticationError => new UnauthorizedResult(),
            ErrorCode.AuthorizationError => new ForbidResult(),
            ErrorCode.ConflictingState => new ForbidResult(),
            _ => new BadRequestObjectResult(error),
        };
    }
}