namespace XWave.Core.Services.Communication;

// todo: replace with ServiceResult<string>
public record AuthenticationResult
{
    public string Error { get; init; } = string.Empty;
    public bool Succeeded { get; init; } = false;
    public string Token { get; init; } = string.Empty;
}