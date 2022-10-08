namespace XWave.Core.Services.Communication;

public record ServiceResult
{
    public bool Succeeded { get; protected init; }

    public Error Error { get; protected init; } = Error.UndefinedError();

    public static ServiceResult Failure(Error error)
    {
        return new ServiceResult()
        {
            Succeeded = false,
            Error = error ,
        };
    }

    /// <summary>
    ///     Helper method that returns an internal failure result with hard-coded message.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult DefaultFailure()
    {
        return Failure(Error.UndefinedError());
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