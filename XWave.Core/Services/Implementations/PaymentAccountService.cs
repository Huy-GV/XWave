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
    private readonly IRoleAuthorizer _roleAuthorizer;
    private readonly ICustomerAccountService _customerAccountService;

    public PaymentAccountService(
        XWaveDbContext dbContext,
        ILogger<PaymentAccountService> logger,
        IRoleAuthorizer roleAuthorizer,
        ICustomerAccountService customerAccountService) : base(dbContext)
    {
        _logger = logger;
        _roleAuthorizer = roleAuthorizer;
        _customerAccountService = customerAccountService;
    }

    public async Task<ServiceResult<int>> AddPaymentAccountAsync(
        string customerId,
        PaymentAccountViewModel inputPayment)
    {
        if (!await _roleAuthorizer.IsUserInRole(customerId, RoleNames.Customer))
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
            });
        }

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
            await DbContext.SaveChangesAsync();
            return ServiceResult<int>.Success(existingPaymentAccount.Id);
        }

        var newPaymentAccount = new PaymentAccount
        {
            AccountNumber = inputPayment.AccountNumber,
            Provider = inputPayment.Provider,
            ExpiryDate = inputPayment.ExpiryDate
        };

        var newTransactionDetails = new PaymentAccountDetails
        {
            CustomerId = customerId,
            Payment = newPaymentAccount,
        };

        DbContext.PaymentAccount.Add(newPaymentAccount);
        DbContext.PaymentAccountDetails.Add(newTransactionDetails);
        await DbContext.SaveChangesAsync();

        return ServiceResult<int>.Success(newPaymentAccount.Id);
    }

    public async Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentAccountId)
    {
        if (!await _roleAuthorizer.IsUserInRole(customerId, RoleNames.Customer))
        {
            return ServiceResult<IEnumerable<PaymentAccount>>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
            });
        }

        var paymentAccountToRemove = await DbContext.PaymentAccount
            .Include(x => x.PaymentAccountDetails)
            .FirstOrDefaultAsync(x => x.PaymentAccountDetails.PaymentAccountId == paymentAccountId);

        if (paymentAccountToRemove is null ||
            paymentAccountToRemove.PaymentAccountDetails.CustomerId != customerId)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Payment account for customer ID {customerId} not found.",
            });
        }

        DbContext.Update(paymentAccountToRemove);
        paymentAccountToRemove.SoftDelete();
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyCollection<PaymentAccount>>> FindAllTransactionDetailsForStaffAsync(string staffId)
    {
        if (!await _roleAuthorizer.IsUserInRoles(
            staffId,
            new [] { RoleNames.Staff, RoleNames.Customer }))
        {
            return ServiceResult<IReadOnlyCollection<PaymentAccount>>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
            });
        }

        var transactions = await DbContext.PaymentAccount
            .AsNoTracking()
            .Include(pd => pd.PaymentAccountDetails)
            .ThenInclude(pd => pd.Customer)
            .ToListAsync();

        return ServiceResult<IReadOnlyCollection<PaymentAccount>>.Success(transactions.AsIReadonlyCollection());
    }

    public async Task<ServiceResult> UpdatePaymentAccountAsync(
        string customerId,
        int paymentAccountId,
        PaymentAccountViewModel updatedPayment)
    {
        var paymentAccount = await DbContext.PaymentAccount
            .Include(x => x.PaymentAccountDetails)
            .FirstOrDefaultAsync(x => x.PaymentAccountDetails.PaymentAccountId == paymentAccountId);

        if (paymentAccount is null ||
            paymentAccount.PaymentAccountDetails.CustomerId != customerId)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Payment account ID {paymentAccountId} not found.",
            });
        }

        DbContext.PaymentAccount
            .Update(paymentAccount)
            .CurrentValues
            .SetValues(updatedPayment);
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

    public async Task<ServiceResult<IReadOnlyCollection<PaymentAccountUsageDto>>> FindAllPaymentAccountsAsync(string customerId)
    {
        if (!await _customerAccountService.CustomerAccountExists(customerId))
        {
            return ServiceResult<IReadOnlyCollection<PaymentAccountUsageDto>>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError,
            });
        }

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

        var paymentAccounts = await DbContext.PaymentAccount
            .Include(p => p.PaymentAccountDetails)
            .Where(p => p.PaymentAccountDetails.CustomerId == customerId)
            .Select(p => new PaymentAccountUsageDto
            {
                Id = p.Id,
                Provider = p.Provider,
                AccountNumber = p.AccountNumber,
                LatestPurchase = purchasesByPaymentAccount.ContainsKey(p.Id)
                    ? purchasesByPaymentAccount[p.Id].Date
                    : null,
                TotalSpending = totalSpendingByPaymentAccount.ContainsKey(p.Id)
                    ? totalSpendingByPaymentAccount[p.Id].TotalSpending
                    : 0,
                PurchaseCount = purchasesByPaymentAccount.ContainsKey(p.Id)
                    ? purchasesByPaymentAccount[p.Id].PurchaseCount
                    : 0

            })
            .ToListAsync();

        // TODO: FIX find function returning 0 TotalSpending even when there are purchases
        return ServiceResult<IReadOnlyCollection<PaymentAccount>>.Success(paymentAccounts.AsIReadonlyCollection());
    }

    public async Task<ServiceResult<PaymentAccountUsageDto>> FindPaymentAccountAsync(int paymentAccountId, string customerId)
    {
        var paymentAccount = await DbContext.PaymentAccount
            .Include(x => x.PaymentAccountDetails)
            .FirstOrDefaultAsync(x => x.PaymentAccountDetails.PaymentAccountId == paymentAccountId);

        if (paymentAccount is null ||
            paymentAccount.PaymentAccountDetails.CustomerId != customerId)
        {
            return ServiceResult<PaymentAccountUsageDto>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Payment account ID {paymentAccountId} not found.",
            });
        }

        var purchasesByCustomer = await DbContext.Order
            .Include(o => o.OrderDetails)
            .Where(o => o.CustomerId == customerId &&
                    o .PaymentAccountId == paymentAccount.Id)
            .OrderByDescending(o => o.Date)
            .ToArrayAsync();

        var totalSpendingOfPaymentAccount = purchasesByCustomer
            .Sum(x => x.OrderDetails.Sum(od => od.Quantity * od.PriceAtOrder));

        var paymentAccountDto = new PaymentAccountUsageDto()
        {
            Id = paymentAccountId,
            Provider = paymentAccount.Provider,
            AccountNumber = paymentAccount.AccountNumber,
            LatestPurchase = purchasesByCustomer.FirstOrDefault()?.Date,
            TotalSpending = totalSpendingOfPaymentAccount,
            PurchaseCount = purchasesByCustomer.Length,
        };

        return ServiceResult<PaymentAccountUsageDto>.Success(paymentAccountDto);
    }
}
