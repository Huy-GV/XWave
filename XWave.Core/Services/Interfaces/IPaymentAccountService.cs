using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Interfaces;

public interface IPaymentAccountService
{
    Task<ServiceResult<int>> AddPaymentAccountAsync(
        string customerId,
        PaymentAccountViewModel newPaymentAccount);

    Task<bool> CustomerHasPaymentAccount(string customerId, int paymentAccountId, bool includeExpiredAccounts = false);

    Task<ServiceResult<IReadOnlyCollection<PaymentAccount>>> FindAllTransactionDetailsForStaffAsync(string staffId);

    Task<ServiceResult<IReadOnlyCollection<PaymentAccountUsageDto>>> FindAllPaymentAccountsAsync(string customerId);

    Task<ServiceResult<PaymentAccountUsageDto>> FindPaymentAccountAsync(int paymentAccountId, string customerId);

    Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentId);

    Task<ServiceResult> UpdatePaymentAccountAsync(
        string customerId,
        int paymentId,
        PaymentAccountViewModel updatedPayment);
}