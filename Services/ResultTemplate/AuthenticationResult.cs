namespace XWave.Services.ResultTemplate
{
    // todo: convert to record
    public class AuthenticationResult
    {
        public string Error { get; set; } = string.Empty;
        public bool Succeeded { get; set; } = false;
        public string Token { get; set; } = string.Empty;
    }
}