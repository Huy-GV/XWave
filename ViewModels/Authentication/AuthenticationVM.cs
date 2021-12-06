namespace XWave.ViewModels.Authentication
{
    public class AuthenticationVM
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        //public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }

    }
}
