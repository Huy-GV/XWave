using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Principal;
using XWave.Data.Constants;

namespace XWave.Helpers
{
    public class AuthenticationHelper
    {
        private ILogger<AuthenticationHelper> _logger;
        public AuthenticationHelper(ILogger<AuthenticationHelper> logger)
        {
            _logger = logger;
        }
        public string GetUserID(IIdentity? identity)
        {
            ClaimsIdentity? claimsIdentity = identity as ClaimsIdentity;
            string userID = claimsIdentity?.FindFirst(CustomClaimType.UserID)?.Value ?? string.Empty;
            
            return userID;
        }
        public string GetUserName(IIdentity? identity)
        {
            ClaimsIdentity? claimsIdentity = identity as ClaimsIdentity;
            string customerID = claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            return customerID;
        }
    }
}
