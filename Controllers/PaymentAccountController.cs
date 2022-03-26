using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XWave.Data;
using XWave.Services;
using XWave.ViewModels;
using XWave.DTOs;
using XWave.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using XWave.Data.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using XWave.Services.Interfaces;
using XWave.Helpers;
using System.Net;
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
        [Authorize(Policy="staffonly")]
        public async Task<ActionResult> Get()
        {
            return Ok(await _paymentService.FindAllTransactionDetailsForStaffAsync());
        }
        [HttpGet("details")]
        [Authorize(Roles ="customer")]
        public ActionResult<IEnumerable<TransactionDetails>> GetByCustomer()
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (customerId == null)
            {
                return XWaveBadRequest("Customer Id is empty");
            }

            return Ok(_paymentService.FindAllTransactionDetailsForCustomersAsync(customerId));
        }
        [HttpPost("delete/{paymentId}")]
        [Authorize(Roles = "customer")]
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
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> UpdatePaymentAsync(int id, [FromBody] PaymentAccountViewModel inputPayment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
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
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> CreatePaymentAsync([FromBody] PaymentAccountViewModel inputPayment)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            var result = await _paymentService.AddPaymentAccountAsync(customerId, inputPayment);

            if (!result.Succeeded)
            { 
                return XWaveBadRequest(result.Error);
            }

            return Ok(XWaveResponse.Created($"https://localhost:5001/api/payment/details/{result.ResourceId}"));
        }
    }
}
