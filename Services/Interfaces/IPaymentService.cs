using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult> CreatePaymentAsync(string customerId, PaymentAccount newPayment);
        Task<ServiceResult> DeletePaymentAsync(string customerId, int paymentId);
        Task<ServiceResult> UpdatePaymentAsync(string customerId, int paymentId, PaymentAccount updatedPayment);
        Task<IEnumerable<TransactionDetails>> GetAllPaymentDetailsForCustomerAsync(string customerId);
        Task<IEnumerable<TransactionDetails>> GetAllPaymentDetailsForStaffAsync();
        Task<bool> CustomerHasPayment(string customerId, int paymentId);
    }
    
}
