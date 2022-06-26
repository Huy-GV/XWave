namespace XWave.Core.Services.Communication;

public record ServiceResult
{
    public bool Succeeded { get; protected init; }

    [Obsolete]
    public IEnumerable<string> ErrorMessages { get; protected init; } = Array.Empty<string>();

    public List<Error> Errors { get; protected init; } = new List<Error>();

    public static ServiceResult Failure(IEnumerable<Error> errors)
    {
        return new ServiceResult()
        {
            Succeeded = false,
            Errors = errors.ToList(),
        };
    }

    public static ServiceResult Failure(Error error)
    {
        return new ServiceResult()
        {
            Succeeded = false,
            Errors = new List<Error> { error },
        };
    }

    /// <summary>
    ///     Helper method that returns a failed result.
    /// </summary>
    /// <param name="errors">Error message describing the cause of failure.</param>
    /// <returns></returns>\
    [Obsolete("Replaced with { Failure(IEnumerable<Error> errors) }")]
    public static ServiceResult Failure(params string[] errors)
    {
        return new() { ErrorMessages = errors };
    }

    /// <summary>
    ///     Helper method that returns an internal failure result with hard-coded message.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult DefaultFailure()
    {
        return new() { Errors = new List<Error> { Error.Default() } };
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