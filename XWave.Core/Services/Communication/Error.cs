namespace XWave.Core.Services.Communication;

public record Error
{
    public string Message { get; init; } = string.Empty;
    public ErrorCode Code { get; init; } = ErrorCode.None;

    public static Error UnknownError() => new()
    {
        Code = ErrorCode.Undefined,
        Message = "Unknown",
    };

    public static Error NoError() => new();
}