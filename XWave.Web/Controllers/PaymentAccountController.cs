using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;
using XWave.Web.Data;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/payment-account")]
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

    [HttpGet("private")]
    [Authorize(Policy = nameof(Policies.InternalPersonnelOnly))]
    public async Task<ActionResult> Get()
    {
        var staffId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.FindAllTransactionDetailsForStaffAsync(staffId);

        return result.OnSuccess(Ok(result.Value));
    }

    [HttpGet()]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult<IEnumerable<PaymentAccountDetails>>> GetAllForCustomer()
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.FindAllPaymentAccountsAsync(customerId);

        return result.OnSuccess(Ok(result.Value));
    }

    [HttpGet("id:int")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult<IEnumerable<PaymentAccountDetails>>> GetByIdForCustomer(int id)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.FindPaymentAccountAsync(id, customerId);

        return result.OnSuccess(Ok(result.Value));
    }

    [HttpPost("delete/{paymentId:int}")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> Delete(int paymentId)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.RemovePaymentAccountAsync(customerId, paymentId);

        return result.OnSuccess(NoContent());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> UpdatePaymentAccountAsync(int id, [FromBody] PaymentAccountViewModel viewModel)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.UpdatePaymentAccountAsync(customerId, id, viewModel);

        // todo: add URL that returns payment account by ID
        return result.OnSuccess(this.Updated($"{this.ApiUrl()}/payment/details/{id}"));
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult> CreatePaymentAsync([FromBody] PaymentAccountViewModel inputPayment)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _paymentService.AddPaymentAccountAsync(customerId, inputPayment);

        return result.OnSuccess(this.Created($"{this.ApiUrl()}/payment/details/{result.Value}"));
    }
}