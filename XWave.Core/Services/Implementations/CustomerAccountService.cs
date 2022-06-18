using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.Services.ResultTemplate;
using XWave.Core.ViewModels.Authentication;
using XWave.Core.ViewModels.Customers;

namespace XWave.Core.Services.Implementations;

internal class CustomerAccountService : ServiceBase, ICustomerAccountService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<CustomerAccountService> _logger;

    public CustomerAccountService(
        XWaveDbContext dbContext,
        IAuthenticationService authenticationService,
        ILogger<CustomerAccountService> logger) : base(dbContext)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    public async Task<AuthenticationResult> RegisterCustomerAsync(RegisterCustomerViewModel viewModel)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var customerAccount = new CustomerAccount();
            var entry = DbContext.CustomerAccount.Add(customerAccount);
            entry.CurrentValues.SetValues(viewModel.CustomerAccountViewModel);
            await DbContext.SaveChangesAsync();
            var authenticationResult = await _authenticationService.RegisterUserAsync(viewModel.UserViewModel);
            if (authenticationResult.Succeeded) await transaction.CommitAsync();

            return authenticationResult;
        }
        catch
        {
            await transaction.RollbackAsync();
            return new AuthenticationResult();
        }
    }

    public async Task<ServiceResult> UpdateSubscriptionAsync(string id, bool isSubscribed)
    {
        try
        {
            var customerAccount = await DbContext.CustomerAccount.FindAsync(id);
            if (customerAccount == null) return ServiceResult.Failure("User account not found.");

            DbContext.CustomerAccount.Update(customerAccount);
            customerAccount.IsSubscribedToPromotions = isSubscribed;
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Failed to update subscription status of customer ID {id}");
            _logger.LogError($"Exception message: {exception.Message}");
            return ServiceResult.InternalFailure();
        }
    }

    public async Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel)
    {
        try
        {
            var customerAccount = await DbContext.CustomerAccount.FindAsync(id);
            if (customerAccount == null) return ServiceResult.Failure("User account not found.");

            var entry = DbContext.CustomerAccount.Update(customerAccount);
            entry.CurrentValues.SetValues(viewModel);
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to update customer account of user ID {id}");
            _logger.LogError($"Exception message: {exception.Message}");
            return ServiceResult.InternalFailure();
        }
    }
}