namespace XWave.Core.Services.Communication;

public record ServiceResult<TResult> : ServiceResult where TResult : notnull
{
    private readonly TResult _value;

    public TResult Value => Succeeded ? _value : throw new InvalidOperationException();

    private ServiceResult(TResult value)
    {
        _value = value;
        Succeeded = true;
        Error = Error.NoError();
    }

    private ServiceResult(Error error)
    {
        _value = default!;
        Succeeded = false;
        Error = error;
    }

    public static ServiceResult<TResultValue> Success<TResultValue>(TResultValue value) where TResultValue : notnull
    {
        return new ServiceResult<TResultValue>(value);
    }

    public static new ServiceResult<TResult> Failure(Error error)
    {
        return new ServiceResult<TResult>(error);
    }

    public static new ServiceResult<TResult> UnknownFailure()
    {
        return Failure(Error.UnknownError());
    }
}