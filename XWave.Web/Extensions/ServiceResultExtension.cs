using Microsoft.AspNetCore.Mvc;
using XWave.Core.Services.Communication;

namespace XWave.Web.Extensions;

public static class ServiceResultExtension
{
    /// <summary>
    /// Map service result to HTTP result with a value based on its error status.
    /// </summary>
    /// <param name="result">Result to map.</param>
    /// <param name="successfulResult">Value to return if result is successful.</param>
    /// <returns>A HTTP status result.</returns>
    public static ActionResult OnSuccess<T>(
        this ServiceResult<T> result, 
        ActionResult successfulResult) where T : notnull
    {
        return result.Succeeded
            ? successfulResult
            : result.Error.MapToHttpResult();
    }

    /// <summary>
    /// Map service result to HTTP result based on its error status.
    /// </summary>
    /// <param name="result">Result to map.</param>
    /// <param name="successfulResult">Value to return if result is successful.</param>
    /// <returns>A HTTP status result.</returns>
    public static ActionResult OnSuccess(
        this ServiceResult result, 
        ActionResult successfulResult)
    {
        return result.Succeeded
            ? successfulResult
            : result.Error.MapToHttpResult();
    }
}