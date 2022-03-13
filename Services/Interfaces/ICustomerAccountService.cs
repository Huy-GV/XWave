using System.Threading.Tasks;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Interfaces
{
    public interface ICustomerAccountService
    {
        public Task<ServiceResult> RegisterCustomerAsync(string id, CustomerAccountViewModel viewModel);
        public Task<ServiceResult> UpdateCustomerAccountAsync(string id, CustomerAccountViewModel viewModel);
    }
}
