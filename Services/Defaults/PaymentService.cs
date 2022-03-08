using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Defaults
{
    public class PaymentService : ServiceBase, IPaymentService
    {
        public PaymentService(XWaveDbContext dbContext) : base(dbContext) { }
        public async Task<ServiceResult> CreatePaymentAsync(string customerId, PaymentAccountViewModel inputPayment)
        {
            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforePaymentCreation";
            transaction.CreateSavepoint(savepoint);

            try
            {
                var newPayment = new PaymentAccount()
                {
                    AccountNumber = inputPayment.AccountNumber,
                    Provider = inputPayment.Provider,
                    ExpiryDate = inputPayment.ExpiryDate,
                };

                DbContext.PaymentAccount.Add(newPayment);
                await DbContext.SaveChangesAsync();

                var newTransactionDetails = new TransactionDetails()
                {
                    CustomerId = customerId,
                    PaymentAccountId = newPayment.Id,
                    Registration = DateTime.Now,
                    PurchaseCount = 0,
                    TransactionType = TransactionType.PaymentAccountRegistration
                };

                DbContext.TransactionDetails.Add(newTransactionDetails);
                await DbContext.SaveChangesAsync();
                transaction.Commit();

                return new ServiceResult
                {
                    Succeeded = true,
                    ResourceId = newPayment.Id.ToString(),
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

        public async Task<ServiceResult> DeletePaymentAsync(string customer, int paymentId)
        {
            try
            {
                var deletedPayment = await DbContext.PaymentAccount.FindAsync(paymentId);
                DbContext.Remove(deletedPayment);
                await DbContext.SaveChangesAsync();

                return new ServiceResult()
                {
                    Succeeded = true,
                    ResourceId = paymentId.ToString(),
                };
            } 
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public Task<IEnumerable<TransactionDetails>> GetAllTransactionDetailsForCustomersAsync(string customerId)
        {
            return Task.FromResult(DbContext.TransactionDetails
                .AsNoTracking()
                .Include(pd => pd.Payment)
                .Where(pd => pd.CustomerId == customerId)
                .AsEnumerable());
        }
        public Task<IEnumerable<TransactionDetails>> GetAllTransactionDetailsForStaffAsync()
        {
            return Task.FromResult(DbContext.TransactionDetails
                .AsNoTracking()
                .Include(pd => pd.Payment)
                .AsEnumerable());
        }

        public async Task<ServiceResult> UpdatePaymentAsync(
            string customerId, 
            int id,
            PaymentAccountViewModel updatedPayment)
        {
            try
            {
                var payment = await DbContext.PaymentAccount.FindAsync(id);
                var entry = DbContext.Attach(payment);
                entry.State = EntityState.Modified;
                entry.CurrentValues.SetValues(updatedPayment);
                DbContext.PaymentAccount.Update(payment);
                await DbContext.SaveChangesAsync();

                return new ServiceResult
                {
                    Succeeded = true,
                    ResourceId = id.ToString(),
                };
            } 
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }

        }
        public Task<bool> CustomerHasPayment(string customerId, int paymentId)
        {
            return Task.FromResult(DbContext.TransactionDetails.Any(
                td => td.CustomerId == customerId && td.PaymentAccountId == paymentId));
        }
    }
}
