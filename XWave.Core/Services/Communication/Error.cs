namespace XWave.Core.Services.Communication;

public record Error
{
    public string Message { get; init; } = "An internal error occurred";
    public ErrorCode ErrorCode { get; init; }

    public static Error Default() => new Error()
    {
        ErrorCode = ErrorCode.Undefined,
    };
}