using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Authentication;
using XWave.ViewModels.Customer;

namespace XWave.Services.Defaults
{
    public class CustomerAccountService : ServiceBase, ICustomerAccountService
    {
        private readonly IAuthenticationService _authenticationService;
        public CustomerAccountService(
            XWaveDbContext dbContext,
            IAuthenticationService authenticationService) : base(dbContext) 
        { 
            _authenticationService = authenticationService;
        }
        public async Task<AuthenticationResult> RegisterCustomerAsync(RegisterCustomerViewModel viewModel)
        {
            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                var customerAccount = new CustomerAccount();
                var entry = DbContext.CustomerAccount.Add(customerAccount);
                entry.CurrentValues.SetValues(viewModel);
                await DbContext.SaveChangesAsync();
                var authenticationResult = await _authenticationService.RegisterUserAsync(viewModel.User);
                if (authenticationResult.Succeeded)
                {
                    await transaction.CommitAsync();
                }

                return authenticationResult;
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                return new AuthenticationResult();
            }
        }

        public async Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel)
        {
            try
            {
                var customerAccount = await DbContext.CustomerAccount.SingleAsync(ca => ca.CustomerId == id);
                var entry = DbContext.CustomerAccount.Update(customerAccount);
                entry.CurrentValues.SetValues(viewModel);
                await DbContext.SaveChangesAsync();

                return ServiceResult.Success(id);
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }
        }
    }
}
