using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using XWave.Configuration;
using XWave.Data.Constants;
using XWave.Helpers;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAccountController : XWaveBaseController
    {
        private readonly ICustomerAccountService _customerAccountService;
        private readonly AuthenticationHelper _authenticationHelper;
        private readonly JwtCookie _jwtCookieConfig;

        public CustomerAccountController(
            AuthenticationHelper authenticationHelper,
            ICustomerAccountService customerAccountService,
            IOptions<JwtCookie> jwtCookieOptions)
        {
            _jwtCookieConfig = jwtCookieOptions.Value;
            _authenticationHelper = authenticationHelper;
            _customerAccountService = customerAccountService;
        }

        [HttpPost("register/customer")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult<AuthenticationResult>> RegisterCustomerAsync([FromBody] RegisterCustomerViewModel viewModel)
        {
            var result = await _customerAccountService.RegisterCustomerAsync(viewModel);
            if (result.Succeeded)
            {
                if (Request.Cookies.ContainsKey(_jwtCookieConfig.Name))
                {
                    Response.Cookies.Delete(_jwtCookieConfig.Name);
                }

                var cookieOptions = _authenticationHelper.CreateCookieOptions(_jwtCookieConfig.DurationInDays);
                Response.Cookies.Append(_jwtCookieConfig.Name, result.Token, cookieOptions);

                return Ok(result);
            }

            return XWaveBadRequest(result.Error);
        }

        [HttpPost("subscribe")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult<ServiceResult>> UpdatePromotionSubscriptionAsync([FromBody] bool isSubscribed = true)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var result = await _customerAccountService.UpdateSubscriptionAsync(customerId, isSubscribed);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return XWaveBadRequest(result.Error);
        }
    }
}