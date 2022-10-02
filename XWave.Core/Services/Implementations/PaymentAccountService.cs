using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Implementations;

internal class PaymentAccountService : ServiceBase, IPaymentAccountService
{
    private readonly ILogger<PaymentAccountService> _logger;
    private readonly IAuthorizationService _authorizationService;

    public PaymentAccountService(
        XWaveDbContext dbContext,
        ILogger<PaymentAccountService> logger,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public async Task<ServiceResult<int>> AddPaymentAccountAsync(
        string customerId,
        PaymentAccountViewModel inputPayment)
    {
        if (! await _authorizationService.IsUserInRole(customerId, Roles.Customer)) 
        {
            return ServiceResult<int>.Failure(new Error 
            {
                ErrorCode = ErrorCode.InvalidUserRequest,
            });
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var existingPaymentAccount = await DbContext.PaymentAccount
                .SingleOrDefaultAsync(x =>
                    x.AccountNumber == inputPayment.AccountNumber &&
                    x.Provider == inputPayment.Provider &&
                    x.IsDeleted);

            if (existingPaymentAccount is not null)
            {
                DbContext.PaymentAccount.Update(existingPaymentAccount);
                existingPaymentAccount.IsDeleted = false;
                existingPaymentAccount.DeleteDate = null;

                return ServiceResult<int>.Success(existingPaymentAccount.Id);
            }

            var newPaymentAccount = new PaymentAccount
            {
                AccountNumber = inputPayment.AccountNumber,
                Provider = inputPayment.Provider,
                ExpiryDate = inputPayment.ExpiryDate
            };

            DbContext.PaymentAccount.Add(newPaymentAccount);
            await DbContext.SaveChangesAsync();

            var newTransactionDetails = new PaymentAccountDetails
            {
                CustomerId = customerId,
                PaymentAccountId = newPaymentAccount.Id
            };

            DbContext.PaymentAccountDetails.Add(newTransactionDetails);
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<int>.Success(newPaymentAccount.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Exception: {exception.Message}");
            await transaction.RollbackAsync();

            return ServiceResult<int>.DefaultFailure();
        }
    }

    public async Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentId)
    {
        if (! await _authorizationService.IsUserInRole(customerId, Roles.Customer)) 
        {
            return ServiceResult<IEnumerable<PaymentAccount>>.Failure(new Error 
            {
                ErrorCode = ErrorCode.InvalidUserRequest,
            });
        }

        var paymentAccountToRemove = await DbContext.PaymentAccount.FindAsync(paymentId);
        if (paymentAccountToRemove is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Payment account for customer ID {customerId} not found.",
            });
        }

        DbContext.Update(paymentAccountToRemove);
        paymentAccountToRemove.SoftDelete();
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IEnumerable<PaymentAccount>>> FindAllTransactionDetailsForStaffAsync(string staffId)
    {
        if (! await _authorizationService.IsUserInRole(staffId, Roles.Staff)) 
        {
            return ServiceResult<IEnumerable<PaymentAccount>>.Failure(new Error 
            {
                ErrorCode = ErrorCode.InvalidUserRequest,
            });
        }

        var transactions = DbContext.PaymentAccount
            .AsNoTracking()
            .Include(pd => pd.PaymentAccountDetails)
            .ThenInclude(pd => pd.Customer)
            .AsEnumerable();

        return ServiceResult<IEnumerable<PaymentAccount>>.Success(transactions);
    }

    public async Task<ServiceResult> UpdatePaymentAccountAsync(
        string customerId,
        int paymentAccountId,
        PaymentAccountViewModel updatedPayment)
    {
        var payment = await DbContext.PaymentAccount.FindAsync(paymentAccountId);
        if (payment is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Payment account ID {paymentAccountId} not found.",
            });
        }

        DbContext.Update(payment).CurrentValues.SetValues(updatedPayment);
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<bool> CustomerHasPaymentAccount(
        string customerId,
        int paymentId,
        bool includeExpiredAccounts = false)
    {
        return await DbContext.PaymentAccountDetails
            .Include(x => x.Payment)
            .AnyAsync(
                x => x.CustomerId == customerId &&
                x.PaymentAccountId == paymentId &&
                (x.Payment.ExpiryDate > DateTime.Now || includeExpiredAccounts));
    }

    public async Task<IEnumerable<PaymentAccountUsageDto>> FindPaymentAccountSummary(string customerId)
    {
        var purchasesByCustomer = await DbContext.Order
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Date)
            .ToArrayAsync();

        var purchasesByPaymentAccount = purchasesByCustomer
            .GroupBy(o => o.PaymentAccountId)
            .Select(g => new
            {
                PaymentAccountId = g.Key,
                Date = g.Max(o => o.Date),
                PurchaseCount = g.Count()
            })
            .ToDictionary(x => x.PaymentAccountId);

        var totalSpendingByPaymentAccount = purchasesByCustomer
            .GroupBy(o => o.PaymentAccountId)
            .Select(g => new
            {
                PaymentAccountId = g.Key,
                TotalSpending = g.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.PriceAtOrder))
            })
            .ToDictionary(x => x.PaymentAccountId);

        return await DbContext.PaymentAccount
            .Include(p => p.PaymentAccountDetails)
            .Where(p => p.PaymentAccountDetails.CustomerId == customerId)
            .Select(p => new PaymentAccountUsageDto
            {
                Provider = p.Provider,
                AccountNumber = p.AccountNumber,
                LatestPurchase = purchasesByPaymentAccount.ContainsKey(p.Id)
                    ? purchasesByPaymentAccount[p.Id].Date
                    : null,
                TotalSpending = totalSpendingByPaymentAccount.ContainsKey(p.Id)
                    ? totalSpendingByPaymentAccount[p.Id].TotalSpending
                    : 0,
                PurchaseCount = purchasesByPaymentAccount.ContainsKey(p.Id)
                    ? (ushort)purchasesByPaymentAccount[p.Id].PurchaseCount
                    : (ushort)0
                
            })
            .ToListAsync();
    }
}