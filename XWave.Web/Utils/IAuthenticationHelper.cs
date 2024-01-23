using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace XWave.Web.Utils;

public interface IAuthenticationHelper
{
    public string GetUserId(IIdentity? identity);

    public string GetUserName(IIdentity? identity);

    public CookieOptions CreateCookieOptions();
}