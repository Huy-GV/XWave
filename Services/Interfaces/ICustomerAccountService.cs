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
        public Task<ServiceResult> UpdateSubscriptionAsync(string id, bool isSubscribed);
    }
}
