using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ServiceResultExtension
{
    public static ActionResult MapResult<T>(
        this ServiceResult<T> result, 
        ActionResult successfulResult) where T : notnull
    {
        return result.Succeeded
            ? successfulResult
            : ResultMapper.MapErrorToHttpCode(result.Error);
    }

    public static ActionResult MapResult(
        this ServiceResult result, 
        ActionResult successfulResult)
    {
        return result.Succeeded
            ? successfulResult
            : ResultMapper.MapErrorToHttpCode(result.Error);
    }
}