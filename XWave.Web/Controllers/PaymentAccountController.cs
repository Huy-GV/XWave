using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Customers;
using XWave.Web.Data;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentAccountController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IPaymentAccountService _paymentService;

    public PaymentAccountController(
        IPaymentAccountService paymentService,
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
            ? Forbid()
            : Ok(await _paymentService.FindPaymentAccountSummary(customerId));
    }

    [HttpPost("delete/{paymentId:int}")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> Delete(int paymentId)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (!await _paymentService.CustomerHasPaymentAccount(customerId, paymentId))
            return BadRequest(XWaveResponse.NonExistentResource());

        var result = await _paymentService.RemovePaymentAccountAsync(customerId, paymentId);

        if (!result.Succeeded)
        {
            return UnprocessableEntity(result.Errors);
        };

        return NoContent();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> UpdatePaymentAccountAsync(int id, [FromBody] PaymentAccountViewModel viewModel)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (!await _paymentService.CustomerHasPaymentAccount(customerId, id))
            return BadRequest(XWaveResponse.NonExistentResource());

        var result = await _paymentService.UpdatePaymentAccountAsync(customerId, id, viewModel);
        return !result.Succeeded
            ? UnprocessableEntity(result.Errors)
            : this.XWaveUpdated($"https://localhost:5001/api/payment/details/{id}");
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> CreatePaymentAsync([FromBody] PaymentAccountViewModel inputPayment)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.AddPaymentAccountAsync(customerId, inputPayment);

        return !result.Succeeded
            ? UnprocessableEntity(result.Errors)
            : this.XWaveCreated($"https://localhost:5001/api/payment/details/{result.Value}");
    }
}