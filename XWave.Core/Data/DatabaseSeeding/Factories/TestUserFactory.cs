using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;
public static class TestUserFactory
{
    public static List<ApplicationUser> Managers()
    {
        var manager1 = new ApplicationUser
        {
            UserName = "matt_manager",
            FirstName = "Matt",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        var manager2 = new ApplicationUser
        {
            UserName = "tom_manager",
            FirstName = "Tom",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        return [manager1, manager2];
    }

    public static List<ApplicationUser> StaffUsers()
    {
        var staff1 = new ApplicationUser
        {
            UserName = "paul_staff",
            FirstName = "Paul",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        var staff2 = new ApplicationUser
        {
            UserName = "liz_staff",
            FirstName = "Elizabeth",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "2345678"
        };

        return [staff1, staff2];
    }

    public static List<ApplicationUser> CustomerUsers()
    {
        var customer1 = new ApplicationUser
        {
            UserName = "john_customer",
            FirstName = "John",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        var customer2 = new ApplicationUser
        {
            UserName = "jake_customer",
            FirstName = "Jake",
            LastName = "Applebee",
            RegistrationDate = DateTime.Now,
            PhoneNumber = "98765432"
        };

        return [customer1, customer2];
    }

    public static List<StaffAccount> StaffAccounts(IEnumerable<ApplicationUser> managers, IEnumerable<ApplicationUser> staffUsers)
    {
        return managers
            .Zip(staffUsers)
            .Select(x => new StaffAccount()
            {
                ContractStartDate = DateTime.UtcNow,
                ImmediateManagerId = x.First.Id,
                StaffId = x.Second.Id,
                Address = "1 Main St, CBD"
            })
            .ToList();
    }
}
