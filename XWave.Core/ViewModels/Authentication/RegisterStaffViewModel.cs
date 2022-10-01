using XWave.Core.ViewModels.Management;

namespace XWave.Core.ViewModels.Authentication;

public class RegisterStaffViewModel
{
    public RegisterUserViewModel User { get; set; } = new();

    public StaffAccountViewModel StaffAccount { get; set; } = new ();
}