﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult> AddPaymentAccountAsync(string customerId, PaymentAccountViewModel newPayment);

        Task<bool> CustomerHasPayment(string customerId, int paymentId);

        Task<IEnumerable<TransactionDetails>> FindAllTransactionDetailsForCustomersAsync(string customerId);

        Task<IEnumerable<TransactionDetails>> FindAllTransactionDetailsForStaffAsync();

        // todo: only remove from the customer's perspective
        Task<ServiceResult> RemovePaymentAccountAsync(string customerId, int paymentId);

        Task<ServiceResult> UpdatePaymentAccountAsync(
                                            string customerId,
            int paymentId,
            PaymentAccountViewModel updatedPayment);
    }
}