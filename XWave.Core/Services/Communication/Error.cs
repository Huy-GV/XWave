namespace XWave.Core.Services.Communication;

public record Error
{
    public string Message { get; init; } = "An internal error occurred";
    public ErrorCode Code { get; init; }

    public static Error Default() => new Error()
    {
        Code = ErrorCode.Undefined,
    };
}