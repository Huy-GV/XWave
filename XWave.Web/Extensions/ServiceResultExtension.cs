using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ServiceResultExtension
{
    public static ActionResult MapResult<T>(
        this ServiceResult<T> result, 
        ActionResult successfulResult,
        Func<Error, ActionResult> errorMapper) where T : notnull
    {
        return result.Succeeded
            ? successfulResult
            : errorMapper(result.Error);
    }

    public static ActionResult MapResult(
        this ServiceResult result, 
        ActionResult successfulResult,
        Func<Error, ActionResult> errorMapper)
    {
        return result.Succeeded
            ? successfulResult
            : errorMapper(result.Error);
    }
}