using Newtonsoft.Json.Linq;

namespace XWave.Core.Services.Communication;

public record ServiceResult
{
    public bool Succeeded { get; protected init; }

    public Error Error { get; protected init; } = Error.UnknownError();

    protected ServiceResult()
    {
        Succeeded = true;
        Error = Error.NoError();
    }

    protected ServiceResult(Error error)
    {
        Succeeded = false;
        Error = error;
    }

    public static ServiceResult Failure(Error error)
    {
        return new ServiceResult(error);
    }

    /// <summary>
    ///     Helper method that returns an internal failure result with hard-coded message.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult UnknownFailure()
    {
        return Failure(Error.UnknownError());
    }

    /// <summary>
    ///     Helper method that returns a success result.
    /// </summary>
    /// <returns></returns>
    public static ServiceResult Success()
    {
        return new();
    }
}