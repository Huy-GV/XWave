using System;
using System.Collections.Generic;

namespace XWave.Services.ResultTemplate;

// convert to record and move methods to helper objects
public record ServiceResult
{
    public bool Succeeded { get; private init; }

    public IEnumerable<string> Errors { get; private init; } = Array.Empty<string>();

    /// <summary>
    ///     Helper method that returns a failed result.
    /// </summary>
    /// <param name="errors">Error message describing the cause of failure.</param>
    /// <returns></returns>
    public static ServiceResult Failure(params string[] errors)
    {
        return new() { Errors = errors };
    }

    /// <summary>
    ///     Helper method that returns an internal failure result with hard-coded message.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult InternalFailure()
    {
        return new() { Errors = new[] { "An internal failure occurred." } };
    }

    /// <summary>
    ///     Helper method that returns a success result.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult Success()
    {
        return new() { Succeeded = true };
    }
}