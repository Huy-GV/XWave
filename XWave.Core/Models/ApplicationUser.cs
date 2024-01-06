using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace XWave.Core.Models;

public class ApplicationUser : IdentityUser
{
    [Required] [Column(TypeName = "date")] public DateTime RegistrationDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(15, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(25, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    public override bool LockoutEnabled { get; set; } = false;

    #region REMOVED FROM TABLE

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [NotMapped] public override string NormalizedEmail { get; set; }

    [NotMapped] public override string NormalizedUserName { get; set; }

    [NotMapped] public override int AccessFailedCount { get; set; }

    [NotMapped] public override string SecurityStamp { get; set; }

    [NotMapped] public override bool TwoFactorEnabled { get; set; }

    [NotMapped] public override bool PhoneNumberConfirmed { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion REMOVED FROM TABLE
}