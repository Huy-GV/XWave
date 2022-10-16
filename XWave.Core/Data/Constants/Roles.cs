namespace XWave.Core.Data.Constants;

public static class RoleNames
{
    public const string Manager = nameof(Manager);
    public const string Staff = nameof(Staff);
    public const string Customer = nameof(Customer);
    public static readonly string[] InternalPersonnelRoles = new [] { Staff, Manager };
}