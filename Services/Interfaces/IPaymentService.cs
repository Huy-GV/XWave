using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult> CreatePaymentAsync(string customerID, Payment newPayment);
        Task<ServiceResult> DeletePaymentAsync(string customerID, int paymentID);
        Task<ServiceResult> UpdatePaymentAsync(string customerID, int paymentID, Payment updatedPayment);
        Task<IEnumerable<PaymentDetail>> GetAllPaymentDetailsForCustomerAsync(string customerID);
        Task<IEnumerable<PaymentDetail>> GetAllPaymentDetailsForStaffAsync();
        Task<bool> CustomerHasPayment(string customerID, int paymentID);
    }
    
}
