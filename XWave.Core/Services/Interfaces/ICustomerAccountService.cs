using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Authentication;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Interfaces;

public interface ICustomerAccountService
{
    Task<ServiceResult<string>> RegisterCustomerAsync(RegisterCustomerViewModel viewModel);

    Task<ServiceResult> UpdateCustomerAccountAsync(string customerId, CustomerAccountViewModel viewModel);

    /// <summary>
    ///     Update subscription to promotion.
    /// </summary>
    /// <param name="customerId">ID of customer.</param>
    /// <param name="isSubscribed">Subscription status.</param>
    /// <returns></returns>
    Task<ServiceResult> UpdateSubscriptionAsync(string customerId, bool isSubscribed);

    Task<bool> CustomerAccountExists(string customerId);
}