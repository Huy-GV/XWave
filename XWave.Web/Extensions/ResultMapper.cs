using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ResultMapper
{
    public static ActionResult MapErrorToHttpCode(Error error)
    {
        return error.Code switch 
        {
            ErrorCode.EntityNotFound => new NotFoundObjectResult(error),
            ErrorCode.AuthenticationError => new UnauthorizedObjectResult(error),
            ErrorCode.AuthorizationError => new ForbidResult(),
            ErrorCode.ConflictingState => new ConflictObjectResult(error),
            _ => new BadRequestObjectResult(error),
        };
    }
}