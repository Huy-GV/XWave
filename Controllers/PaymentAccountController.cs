using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Utils;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.ViewModels.Customer;
using XWave.Extensions;
using System.Linq;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentAccountController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AuthenticationHelper _authenticationHelper;

        public PaymentAccountController(
            IPaymentService paymentService,
            AuthenticationHelper authenticationHelper)
        {
            _paymentService = paymentService;
            _authenticationHelper = authenticationHelper;
        }

        [HttpGet]
        [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
        public async Task<ActionResult> Get()
        {
            return Ok(await _paymentService.FindAllTransactionDetailsForStaffAsync());
        }

        [HttpGet("usage")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult<IEnumerable<PaymentAccountDetails>>> GetByCustomer()
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            return string.IsNullOrEmpty(customerId)
                ? this.XWaveBadRequest("Customer Id is empty.")
                : Ok(await _paymentService.FindPaymentAccountSummary(customerId));
        }

        [HttpPost("delete/{paymentId:int}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> Delete(int paymentId)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!await _paymentService.CustomerHasPaymentAccount(customerId, paymentId))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _paymentService.RemovePaymentAccountAsync(customerId, paymentId);

            if (!result.Succeeded)
            {
                return this.XWaveBadRequest(result.Errors.ToArray());
            }

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> UpdatePaymentAsync(int id, [FromBody] PaymentAccountViewModel inputPayment)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!await _paymentService.CustomerHasPaymentAccount(customerId, id))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _paymentService.UpdatePaymentAccountAsync(customerId, id, inputPayment);
            return !result.Succeeded
                ? this.XWaveBadRequest(result.Errors.ToArray())
                : Ok(XWaveResponse.Updated($"https://localhost:5001/api/payment/details/{id}"));
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> CreatePaymentAsync([FromBody] PaymentAccountViewModel inputPayment)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var (result, paymentAccountId) = await _paymentService.AddPaymentAccountAsync(customerId, inputPayment);

            return !result.Succeeded
                ? this.XWaveBadRequest(result.Errors.ToArray())
                : Ok(XWaveResponse.Created($"https://localhost:5001/api/payment/details/{paymentAccountId}"));
        }
    }
}