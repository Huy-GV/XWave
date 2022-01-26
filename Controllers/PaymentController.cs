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
using System.Net;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : AbstractController<PaymentController>
    {
        private readonly IAuthenticationService _authService;
        private readonly IPaymentService _paymentService;
        public PaymentController(
            ILogger<PaymentController> logger,
            IAuthenticationService authService,
            IPaymentService paymentService) : base(logger)
        {
            _authService = authService;
            _paymentService = paymentService;
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
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            if (customerID == null)
                return BadRequest();

            return Ok(_paymentService.GetAllPaymentDetailsForCustomerAsync(customerID));
        }
        [HttpPost("delete/{paymentID}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> Delete(int paymentID)
        {
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            
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
                return BadRequest(ModelState);

            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            var result = await _paymentService.UpdatePaymentAsync(customerID, id, inputPayment);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(ResponseTemplate.Updated($"https://localhost:5001/api/payment/details/{id}"));
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> CreatePaymentAsync(Payment inputPayment)
        {
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentService.CreatePaymentAsync(customerID, inputPayment);

            if (!result.Succeeded)
            { 
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError, 
                    ResponseTemplate.InternalServerError(result.Error));
            }

            return Ok(ResponseTemplate.Created($"https://localhost:5001/api/payment/details/{result.ResourceID}"));
        }
    }
}
