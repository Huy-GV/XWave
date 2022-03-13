using System.Threading.Tasks;
using XWave.Data;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Defaults
{
    public class CustomerAccountService : ServiceBase, ICustomerAccountService
    {
        public CustomerAccountService(XWaveDbContext dbContext) : base(dbContext) { }
        public Task<ServiceResult> RegisterCustomerAsync(string id, CustomerAccountViewModel viewModel)
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel)
        {
            throw new System.NotImplementedException();
        }
    }
}
