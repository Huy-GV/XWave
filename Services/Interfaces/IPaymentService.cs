using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult> CreatePaymentAsync(string customerID, PaymentAccount newPayment);
        Task<ServiceResult> DeletePaymentAsync(string customerID, int paymentID);
        Task<ServiceResult> UpdatePaymentAsync(string customerID, int paymentID, PaymentAccount updatedPayment);
        Task<IEnumerable<TransactionDetails>> GetAllPaymentDetailsForCustomerAsync(string customerID);
        Task<IEnumerable<TransactionDetails>> GetAllPaymentDetailsForStaffAsync();
        Task<bool> CustomerHasPayment(string customerID, int paymentID);
    }
    
}
