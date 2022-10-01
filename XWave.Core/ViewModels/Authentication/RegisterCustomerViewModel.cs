using XWave.Core.ViewModels.Customers;

namespace XWave.Core.ViewModels.Authentication;

public class RegisterCustomerViewModel
{
    public RegisterUserViewModel UserViewModel { get; set; } = new();

    public CustomerAccountViewModel CustomerAccountViewModel { get; set; } = new();
}