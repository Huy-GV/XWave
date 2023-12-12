using Microsoft.AspNetCore.Identity;
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
    private readonly IAuthenticator _authenticator;
    private readonly ILogger<CustomerAccountService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerAccountService(
        XWaveDbContext dbContext,
        IAuthenticator authenticator,
        ILogger<CustomerAccountService> logger,
        UserManager<ApplicationUser> userManager) : base(dbContext)
    {
        _logger = logger;
        _authenticator = authenticator;
        _userManager = userManager;
    }

    public async Task<ServiceResult<string>> RegisterCustomerAsync(RegisterCustomerViewModel viewModel)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var customerAccount = new CustomerAccount();
            DbContext.CustomerAccount
                .Add(customerAccount)
                .CurrentValues
                .SetValues(viewModel.CustomerAccountViewModel);

            await DbContext.SaveChangesAsync();
            var authenticationResult = await _authenticator
                .RegisterUserAsync(viewModel.UserViewModel);

            if (authenticationResult.Succeeded)
            {
                await transaction.CommitAsync();
                return ServiceResult<string>.Success(authenticationResult.Value ?? string.Empty);
            }

            return authenticationResult;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Failed to register user account.");
            _logger.LogDebug(ex, ex.Message);
            return ServiceResult<string>.UnknownFailure();
        }
        finally {
            await transaction.RollbackAsync();
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

        DbContext.CustomerAccount.Update(customerAccount);
        customerAccount.IsSubscribedToPromotions = isSubscribed;
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
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

        DbContext.CustomerAccount
            .Update(customerAccount)
            .CurrentValues.SetValues(viewModel);
        await DbContext.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<bool> CustomerAccountExists(string id)
    {
        var userExists = await _userManager.Users
            .Select(x => x.Id)
            .AnyAsync(x => x == id);

        return userExists && await DbContext.CustomerAccount.AnyAsync(x => x.CustomerId == id);
    }
}
