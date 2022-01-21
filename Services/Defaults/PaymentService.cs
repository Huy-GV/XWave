using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;

namespace XWave.Services.Defaults
{
    public class PaymentService : ServiceBase, IPaymentService
    {
        public PaymentService(XWaveDbContext dbContext) : base(dbContext) { }
        public async Task<bool> CreatePayment(string customerID, Payment inputPayment)
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

                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<bool> DeletePayment(string customerID, int paymentID)
        {
            if (!CustomerHasPayment(customerID, paymentID))
                return false;

            var deletedPayment = await DbContext.Payment.FindAsync(paymentID);
            DbContext.Remove(deletedPayment);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public Task<IEnumerable<PaymentDetail>> GetAllPaymentDetails(string customerID)
        {
            return Task.FromResult(DbContext.PaymentDetail
                .Include(pd => pd.Payment)
                .Where(pd => pd.CustomerID == customerID)
                .AsEnumerable());
        }

        public async Task<bool> UpdatePayment(string customerID, int id, Payment updatedPayment)
        {
            if (!CustomerHasPayment(customerID, id))
                return false;

            var payment = await DbContext.Payment.FindAsync(id);
            if (payment == null)
                return false;

            payment.AccountNo = updatedPayment.AccountNo;
            payment.Provider = updatedPayment.Provider;
            payment.ExpiryDate = updatedPayment.ExpiryDate;
            DbContext.Payment.Update(payment);
            await DbContext.SaveChangesAsync();
            return true;
        }
        private bool CustomerHasPayment(string customerID, int paymentID)
        {
            return DbContext.PaymentDetail.Any(
                pd => pd.CustomerID == customerID && pd.PaymentID == paymentID);
        }
    }
}
