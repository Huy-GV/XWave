namespace XWave.Services.ResultTemplate
{
    public record AuthenticationResult
    {
        public string Error { get; set; } = string.Empty;
        public bool Succeeded { get; set; } = false;
        public string Token { get; set; } = string.Empty;
    }
}