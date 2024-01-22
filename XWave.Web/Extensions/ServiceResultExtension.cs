using Microsoft.AspNetCore.Mvc;
using System;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ServiceResultExtension
{
    /// <summary>
    /// Map service result to HTTP result with a value based on its error status.
    /// </summary>
    /// <param name="result">Result to map.</param>
    /// <param name="convertToHttpResult">Function that converts successful service result to HTTP code.</param>
    /// <returns>A HTTP status result.</returns>
    public static ActionResult OnSuccess<T>(
        this ServiceResult<T> result,
        Func<T, ActionResult> convertToHttpResult) where T : notnull
    {
        return result.Succeeded
            ? convertToHttpResult(result.Value)
            : result.Error.MapToHttpResult();
    }

    /// <summary>
    /// Map service result to HTTP result based on its error status.
    /// </summary>
    /// <param name="result">Result to map.</param>
    /// <param name="convertToHttpResult">Function that converts successful service result to HTTP code.</param>
    /// <returns>A HTTP status result.</returns>
    public static ActionResult OnSuccess(
        this ServiceResult result,
        Func<ActionResult> convertToHttpResult)
    {
        return result.Succeeded
            ? convertToHttpResult()
            : result.Error.MapToHttpResult();
    }
}