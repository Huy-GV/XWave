using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Interfaces;

public interface IPaymentService
{
    Task<ServiceResult<int>> AddPaymentAccountAsync(string customerId,
        PaymentAccountViewModel newPaymentAccount);

    Task<bool> CustomerHasPaymentAccount(string customerId, int paymentAccountId);

    Task<IEnumerable<PaymentAccount>> FindAllTransactionDetailsForStaffAsync();

    /// <summary>
    /// Find the summaraized usage history of a payment account.
    /// </summary>
    /// <param name="customerId">ID of the payment account owner.</param>
    /// <returns>An enumerable of <see cref="PaymentAccountUsageDto"/> which contains account usage information.</returns>
    Task<IEnumerable<PaymentAccountUsageDto>> FindPaymentAccountSummary(string customerId);

    Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentId);

    Task<ServiceResult> UpdatePaymentAccountAsync(
        string customerId,
        int paymentId,
        PaymentAccountViewModel updatedPayment);
}