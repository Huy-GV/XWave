﻿using XWave.Core.DTOs.Customers;
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

    /// <summary>
    /// Find the summarized usage history of a payment account.
    /// </summary>
    /// <param name="customerId">ID of the payment account owner.</param>
    /// <returns>An enumerable of <see cref="PaymentAccountUsageDto"/> which contains account usage information.</returns>
    Task<ServiceResult<IReadOnlyCollection<PaymentAccountUsageDto>>> FindPaymentAccountSummary(string customerId);

    Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentId);

    Task<ServiceResult> UpdatePaymentAccountAsync(
        string customerId,
        int paymentId,
        PaymentAccountViewModel updatedPayment);
}