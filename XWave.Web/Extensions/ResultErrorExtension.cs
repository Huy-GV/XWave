using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ResultErrorExtension
{
    public static ActionResult MapToHttpResult(this Error error)
    {
        return error.Code switch 
        {
            ErrorCode.EntityNotFound => new NotFoundObjectResult(new { error }),
            ErrorCode.AuthenticationError => new UnauthorizedObjectResult(new { error }),
            ErrorCode.AuthorizationError => new ForbidResult(),
            ErrorCode.ConflictingState => new ConflictObjectResult(new { error }),
            _ => new BadRequestObjectResult(new { error }),
        };
    }
}