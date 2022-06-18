using XWave.Core.Services.ResultTemplate;
using XWave.Core.ViewModels.Authentication;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Interfaces;

public interface ICustomerAccountService
{
    Task<AuthenticationResult> RegisterCustomerAsync(RegisterCustomerViewModel viewModel);
    Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel);

    /// <summary>
    ///     Update subscription to promotion.
    /// </summary>
    /// <param name="customerId">ID of customer.</param>
    /// <param name="isSubscribed">Subscription status.</param>
    /// <returns></returns>
    Task<ServiceResult> UpdateSubscriptionAsync(string customerId, bool isSubscribed);
}