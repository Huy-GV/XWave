namespace XWave.Services.ResultTemplate
{
    public class AuthenticationResult
    {
        public string Error { get; set; } = string.Empty;
        public bool Succeeded { get; set; } = false;
        //public string UserName { get; set; }
        //public string Email { get; set; }
        //public string Role { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
