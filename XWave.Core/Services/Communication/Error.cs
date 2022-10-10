namespace XWave.Core.Services.Communication;

public record Error
{
    public string Message { get; init; } = string.Empty;
    public ErrorCode Code { get; init; } = ErrorCode.None;

    public static Error UndefinedError() => new Error()
    {
        Code = ErrorCode.Undefined,
        Message = "An internal error occurred",
    };

    public static Error Empty() => new Error();
}