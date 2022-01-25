using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class PaymentService : ServiceBase, IPaymentService
    {
        public PaymentService(XWaveDbContext dbContext) : base(dbContext) { }
        public async Task<ServiceResult> CreatePaymentAsync(string customerID, Payment inputPayment)
        {
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

                return new ServiceResult
                {
                    Succeeded = true,
                    ResourceID = newPayment.ID.ToString(),
                };
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ServiceResult
                {
                    Error = ex.Message,
                };
            }
        }

        public async Task<ServiceResult> DeletePaymentAsync(string customerID, int paymentID)
        {
            if (!CustomerHasPayment(customerID, paymentID))
            {
                return new ServiceResult()
                {
                    Error = $"Payment with ID {paymentID} not found for customer ID {customerID}"
                };
            }

            try
            {
                var deletedPayment = await DbContext.Payment.FindAsync(paymentID);
                DbContext.Remove(deletedPayment);
                await DbContext.SaveChangesAsync();
                return new ServiceResult()
                {
                    Succeeded = true,
                    ResourceID = paymentID.ToString(),
                };
            } catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }

        }

        public Task<IEnumerable<PaymentDetail>> GetAllPaymentDetailsAsync(string customerID)
        {
            return Task.FromResult(DbContext.PaymentDetail
                .Include(pd => pd.Payment)
                .Where(pd => pd.CustomerID == customerID)
                .AsEnumerable());
        }

        public async Task<ServiceResult> UpdatePaymentAsync(string customerID, int id, Payment updatedPayment)
        {
            if (!CustomerHasPayment(customerID, id))
            {
                return new ServiceResult
                {
                    Error = $"No payment with ID {id} found for customer ID {customerID}"
                };
            }
                
            try
            {
                var payment = await DbContext.Payment.FindAsync(id);
                payment.AccountNo = updatedPayment.AccountNo;
                payment.Provider = updatedPayment.Provider;
                payment.ExpiryDate = updatedPayment.ExpiryDate;
                DbContext.Payment.Update(payment);
                await DbContext.SaveChangesAsync();
                return new ServiceResult
                {
                    Succeeded = true,
                    ResourceID = id.ToString(),
                };
            } catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }

        }
        private bool CustomerHasPayment(string customerID, int paymentID)
        {
            return DbContext.PaymentDetail.Any(
                pd => pd.CustomerID == customerID && pd.PaymentID == paymentID);
        }
    }
}
