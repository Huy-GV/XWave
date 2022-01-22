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
using XWave.Services.Interfaces;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : AbstractController<PaymentController>
    {
        private readonly AuthenticationService _authService;
        private readonly IPaymentService _paymentService;
        public PaymentController(
            ILogger<PaymentController> logger,
            XWaveDbContext dbContext,
            AuthenticationService authService,
            IPaymentService paymentService) : base(dbContext, logger)
        {
            _authService = authService;
            _paymentService = paymentService;
        }
        [HttpGet]
        [Authorize(Policy="staffonly")]
        public ActionResult Get()
        {
            return Ok(DbContext.PaymentDetail.ToList());
        }
        [HttpGet("detail")]
        [Authorize(Roles ="customer")]
        public ActionResult<IEnumerable<PaymentDetail>> GetByCustomer()
        {
            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            if (customerID == null)
                return BadRequest();

            return Ok(_paymentService.GetAllPaymentDetailsAsync(customerID));
        }
        [HttpPost("delete/{paymentID}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> Delete(int paymentID)
        {
            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            
            var succeeded = await _paymentService.DeletePaymentAsync(customerID, paymentID);
            
            if (!succeeded)
            {
                return BadRequest();
            }

            return Ok(ResponseTemplate.Deleted(paymentID.ToString(), nameof(Payment)));
            
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> UpdatePaymentAsync(int id, Payment inputPayment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            var succeeded = await _paymentService.UpdatePaymentAsync(customerID, id, inputPayment);
            if (!succeeded)
            {
                return BadRequest();
            }


            return Ok(ResponseTemplate.Updated($"https://localhost:5001/api/payment/detail/{id}"));
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> CreatePaymentAsync(Payment inputPayment)
        {
            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentService.CreatePaymentAsync(customerID, inputPayment);

            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforePaymentCreation";
            transaction.CreateSavepoint(savepoint);

            try
            {
                var newPayment = new Payment()
                {
                    AccountNo = inputPayment.AccountNo,
                    Provider = inputPayment.Provider,
                    ExpiryDate = inputPayment.ExpiryDate,
                };
                DbContext.Payment.Add(newPayment);
                await DbContext.SaveChangesAsync();

                var newPaymentDetail = new PaymentDetail()
                {
                    CustomerID = customerID,
                    PaymentID = newPayment.ID,
                    Registration = DateTime.Now,
                    PurchaseCount = 0,
                    LatestPurchase = null,
                };

                DbContext.PaymentDetail.Add(newPaymentDetail);
                await DbContext.SaveChangesAsync();
                transaction.Commit();

                return Ok(ResponseTemplate.Created($"https://localhost:5001/api/payment/detail/"));
            } catch (Exception exception)
            {
                transaction.Rollback();
                Logger.LogError(exception.Message);
                Logger.LogError(exception.StackTrace);
                return StatusCode(500, ResponseTemplate.InternalServerError());
            }

        }
    }
}
