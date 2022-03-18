using XWave.ViewModels.Customer;

namespace XWave.ViewModels.Authentication
{
    public class RegisterCustomerViewModel
    {
        public RegisterUserViewModel User { get; set; }
        // todo: use in user account management
        public CustomerAccountViewModel CustomerAccount { get; set; }
    }
}
