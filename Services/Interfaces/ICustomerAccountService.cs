using System.Threading.Tasks;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;
using XWave.ViewModels.Customer;

namespace XWave.Services.Interfaces
{
    public interface ICustomerAccountService
    {
        public Task<AuthenticationResult> RegisterCustomerAsync(RegisterCustomerViewModel viewModel);
        public Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel);
        /// <summary>
        /// Update subscription to promotion.
        /// </summary>
        /// <param name="customerId">ID of customer.</param>
        /// <param name="isSubscribed">Subscription status.</param>
        /// <returns></returns>
        public Task<ServiceResult> UpdateSubscriptionAsync(string customerId, bool isSubscribed);
    }
}
