using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.Helpers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.ViewModels.Customer;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentAccountController : XWaveBaseController
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

        [HttpGet("details")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public ActionResult<IEnumerable<TransactionDetails>> GetByCustomer()
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (customerId == null)
            {
                return XWaveBadRequest("Customer Id is empty.");
            }

            return Ok(_paymentService.FindAllTransactionDetailsForCustomersAsync(customerId));
        }

        [HttpPost("delete/{paymentId}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> Delete(int paymentId)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!await _paymentService.CustomerHasPaymentAccount(customerId, paymentId))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _paymentService.RemovePaymentAccountAsync(customerId, paymentId);

            if (!result.Succeeded)
            {
                return XWaveBadRequest(result.Error);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> UpdatePaymentAsync(int id, [FromBody] PaymentAccountViewModel inputPayment)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!await _paymentService.CustomerHasPaymentAccount(customerId, id))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _paymentService.UpdatePaymentAccountAsync(customerId, id, inputPayment);
            if (!result.Succeeded)
            {
                return XWaveBadRequest(result.Error);
            }

            return Ok(XWaveResponse.Updated($"https://localhost:5001/api/payment/details/{id}"));
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult> CreatePaymentAsync([FromBody] PaymentAccountViewModel inputPayment)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            var (result, paymentAccountId) = await _paymentService.AddPaymentAccountAsync(customerId, inputPayment);

            if (!result.Succeeded)
            {
                return XWaveBadRequest(result.Error);
            }

            return Ok(XWaveResponse.Created($"https://localhost:5001/api/payment/details/{paymentAccountId}"));
        }
    }
}