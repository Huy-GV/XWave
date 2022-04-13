using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Customers;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Defaults
{
    public class PaymentService : ServiceBase, IPaymentService
    {
        public PaymentService(XWaveDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(ServiceResult, int? PaymentAccountId)> AddPaymentAccountAsync(string customerId, PaymentAccountViewModel inputPayment)
        {
            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                var existingPaymentAccount = await DbContext.PaymentAccount
                    .SingleOrDefaultAsync(x =>
                    x.AccountNumber == inputPayment.AccountNumber &&
                    x.Provider == inputPayment.Provider &&
                    x.IsDeleted);

                if (existingPaymentAccount != null)
                {
                    DbContext.PaymentAccount.Update(existingPaymentAccount);
                    existingPaymentAccount.IsDeleted = false;
                    existingPaymentAccount.DeleteDate = null;

                    return (ServiceResult.Success(), existingPaymentAccount.Id);
                }

                var newPayment = new PaymentAccount()
                {
                    AccountNumber = inputPayment.AccountNumber,
                    Provider = inputPayment.Provider,
                    ExpiryDate = inputPayment.ExpiryDate,
                };

                DbContext.PaymentAccount.Add(newPayment);
                await DbContext.SaveChangesAsync();

                var newTransactionDetails = new PaymentAccountDetails()
                {
                    CustomerId = customerId,
                    PaymentAccountId = newPayment.Id,
                };

                DbContext.PaymentAccountDetails.Add(newTransactionDetails);
                await DbContext.SaveChangesAsync();
                transaction.Commit();

                return (ServiceResult.Success(), newPayment.Id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return (ServiceResult.Failure("Failed to add payment account"), null);
            }
        }

        public async Task<ServiceResult> RemovePaymentAccountAsync(string customer, int paymentId)
        {
            try
            {
                var deletedPayment = await DbContext.PaymentAccount.FindAsync(paymentId);
                if (deletedPayment == null)
                {
                    return ServiceResult.Failure("Payment account could not be found.");
                }

                DbContext.Update(deletedPayment);
                deletedPayment.DeleteDate = DateTime.Now;
                deletedPayment.IsDeleted = true;
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public Task<IEnumerable<PaymentAccount>> FindAllTransactionDetailsForStaffAsync()
        {
            return Task.FromResult(DbContext.PaymentAccount
                .AsNoTracking()
                .Include(pd => pd.PaymentAccountDetails)
                .ThenInclude(pd => pd.Customer)
                .AsEnumerable());
        }

        public async Task<ServiceResult> UpdatePaymentAccountAsync(
            string customerId,
            int id,
            PaymentAccountViewModel updatedPayment)
        {
            try
            {
                var payment = await DbContext.PaymentAccount.FindAsync(id);
                if (payment == null)
                {
                    return ServiceResult.Failure($"Payment account for user ID {} not found.");
                }

                var entry = DbContext.Update(payment);
                entry.CurrentValues.SetValues(updatedPayment);
                DbContext.PaymentAccount.Update(payment);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public Task<bool> CustomerHasPaymentAccount(string customerId, int paymentId)
        {
            return Task.FromResult(DbContext.PaymentAccountDetails.Any(
                td => td.CustomerId == customerId && td.PaymentAccountId == paymentId));
        }

        public async Task<IEnumerable<PaymentAccountUsageDto>> FindPaymentAccountSummary(string customerId)
        {
            var orders = DbContext.Order
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customerId)
                .AsEnumerable();

            var latestPurchase = orders
                .GroupBy(o => o.PaymentAccountId)
                .Select(g => new
                {
                    PaymentAccountId = g.Key,
                    Date = g.Max(o => o.Date)
                })
                .ToDictionary(x => x.PaymentAccountId);

            var totalSpending = orders
                .GroupBy(o => o.PaymentAccountId)
                .Select(g => new
                {
                    PaymentAccountId = g.Key,
                    TotalSpending = g.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.PriceAtOrder))
                })
                .ToDictionary(x => x.PaymentAccountId); ;

            return await DbContext.PaymentAccount
                .Include(p => p.PaymentAccountDetails)
                .Where(p => p.PaymentAccountDetails.CustomerId == customerId)
                .Select(p => new PaymentAccountUsageDto
                {
                    Provider = p.Provider,
                    AccountNumber = p.AccountNumber,
                    LatestPurchase = latestPurchase[p.Id].Date,
                    TotalSpending = (int)totalSpending[p.Id].TotalSpending
                })
                .ToListAsync();
        }
    }
}