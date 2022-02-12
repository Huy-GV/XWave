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

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : AbstractController<PaymentController>
    {
        private readonly IPaymentService _paymentService;
        private readonly AuthenticationHelper _authenticationHelper;
        public PaymentController(
            ILogger<PaymentController> logger,
            IPaymentService paymentService,
            AuthenticationHelper authenticationHelper) : base(logger)
        {
            _paymentService = paymentService;
            _authenticationHelper = authenticationHelper;
        }
        [HttpGet]
        [Authorize(Policy="staffonly")]
        public async Task<ActionResult> Get()
        {
            return Ok(await _paymentService.GetAllPaymentDetailsForStaffAsync());
        }
        [HttpGet("details")]
        [Authorize(Roles ="customer")]
        public ActionResult<IEnumerable<PaymentDetail>> GetByCustomer()
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (customerID == null)
                return BadRequest();

            return Ok(_paymentService.GetAllPaymentDetailsForCustomerAsync(customerID));
        }
        [HttpPost("delete/{paymentID}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> Delete(int paymentID)
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (! await _paymentService.CustomerHasPayment(customerID, paymentID))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }

            var result = await _paymentService.DeletePaymentAsync(customerID, paymentID);
            
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return NoContent();
            
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> UpdatePaymentAsync(int id, Payment inputPayment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (!await _paymentService.CustomerHasPayment(customerID, id))
            {
                return BadRequest(XWaveResponse.NonExistentResource());
            }
            var result = await _paymentService.UpdatePaymentAsync(customerID, id, inputPayment);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(XWaveResponse.Updated($"https://localhost:5001/api/payment/details/{id}"));
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> CreatePaymentAsync(Payment inputPayment)
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentService.CreatePaymentAsync(customerID, inputPayment);

            if (!result.Succeeded)
            { 
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError, 
                    XWaveResponse.Failed(result.Error));
            }

            return Ok(XWaveResponse.Created($"https://localhost:5001/api/payment/details/{result.ResourceID}"));
        }
    }
}
