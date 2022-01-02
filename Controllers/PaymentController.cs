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

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : AbstractController<PaymentController>
    {
        private readonly AuthenticationService _authService;
        public PaymentController(
            ILogger<PaymentController> logger,
            XWaveDbContext dbContext,
            AuthenticationService authService) : base(dbContext, logger)
        {
            _authService = authService;
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

            return Ok(DbContext.PaymentDetail
                .Include(pd => pd.Payment)
                .Where(pd => pd.CustomerID == customerID)
                .ToList());
        }
        [HttpPost("delete/{paymentID}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> Delete(int paymentID)
        {
            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            if (!DbContext.PaymentDetail.Any(pd => 
            pd.PaymentID == paymentID && pd.CustomerID == customerID))
            {
                return BadRequest();
            }

            var deletedPayment = await DbContext.Payment.FindAsync(paymentID);
            DbContext.Remove(deletedPayment);
            await DbContext.SaveChangesAsync();
            return Ok(ResponseTemplate.Deleted(paymentID.ToString(), nameof(Payment)));
            
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> UpdatePaymentAsync(int id, Payment inputPayment)
        {
            var payment = await DbContext.Payment.FindAsync(id);
            if (payment == null)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            payment.AccountNo = inputPayment.AccountNo;
            payment.Provider = inputPayment.Provider;
            payment.ExpiryDate = inputPayment.ExpiryDate;
            DbContext.Payment.Update(payment);
            await DbContext.SaveChangesAsync();
            return Ok(ResponseTemplate.Updated($"https://localhost:5001/api/payment/detail/"));
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> CreatePaymentAsync(Payment inputPayment)
        {
            string customerID = _authService.GetCustomerID(HttpContext.User.Identity);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
