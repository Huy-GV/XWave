namespace XWave.Core.Services.Communication;

public record ServiceResult<TResult> : ServiceResult where TResult : notnull
{
    public TResult? Value { get; init; }

    public static ServiceResult<TResultValue> Success<TResultValue>(TResultValue value) where TResultValue : notnull
    {
        return new ServiceResult<TResultValue>()
        {
            Value = value,
            Succeeded = true,
        };
    }
    public static new ServiceResult<TResult> Failure(Error error)
    {
        return new ServiceResult<TResult>()
        {
            Value = default,
            Succeeded = false,
            Error =  error ,
        };
    }

    public static new ServiceResult<TResult> DefaultFailure()
    {
        return Failure(Error.Default());
    }
}