using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> CreatePayment(string customerID, Payment newPayment);
        Task<bool> DeletePayment(string customerID, int paymentID);
        Task<bool> UpdatePayment(string customerID, int paymentID, Payment updatedPayment);
        Task<IEnumerable<PaymentDetail>> GetAllPaymentDetails(string customerID);
    }
}
