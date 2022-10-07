using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
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

    public async Task<ServiceResult<string>> RegisterCustomerAsync(RegisterCustomerViewModel viewModel)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var customerAccount = new CustomerAccount();
            var entry = DbContext.CustomerAccount.Add(customerAccount);
            entry.CurrentValues.SetValues(viewModel.CustomerAccountViewModel);
            await DbContext.SaveChangesAsync();
            var authenticationResult = await _authenticationService.RegisterUserAsync(viewModel.UserViewModel);
            if (authenticationResult.Succeeded)
            {
                await transaction.CommitAsync();
                return ServiceResult<string>.Success(authenticationResult.Value ?? string.Empty);
            }

            return authenticationResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogInformation("Failed to register user account.");
            _logger.LogDebug(ex, ex.Message);
            return ServiceResult<string>.DefaultFailure();
        }
    }

    public async Task<ServiceResult> UpdateSubscriptionAsync(string id, bool isSubscribed)
    {
        var customerAccount = await DbContext.CustomerAccount.FindAsync(id);
        if (customerAccount is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Customer account with ID {id} not found.",
            });
        }

        try
        {
            DbContext.CustomerAccount.Update(customerAccount);
            customerAccount.IsSubscribedToPromotions = isSubscribed;
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Failed to update subscription status of customer ID {id}");
            _logger.LogError($"Exception message: {exception.Message}");
            return ServiceResult.DefaultFailure();
        }
    }

    public async Task<ServiceResult> UpdateCustomerAccountAsync(
        string id, 
        CustomerAccountViewModel viewModel)
    {
        var customerAccount = await DbContext.CustomerAccount.FindAsync(id);
        if (customerAccount is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Customer account with ID {id} not found.",
            });
        }

        try
        {
            DbContext.CustomerAccount
                .Update(customerAccount)
                .CurrentValues.SetValues(viewModel);
            await DbContext.SaveChangesAsync();

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to update customer account of user ID {id}");
            _logger.LogError($"Exception message: {exception.Message}");
            return ServiceResult.DefaultFailure();
        }
    }

    public async Task<bool> CustomerAccountExists(string id)
    {
        return await _authenticationService.UserExists(id) &&
            await DbContext.CustomerAccount.AnyAsync(x => x.CustomerId == id);
    }
}