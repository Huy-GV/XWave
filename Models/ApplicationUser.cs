﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime RegistrationDate { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string LastName { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string Address { get; set; }
        [StringLength(20, MinimumLength = 1)]
        public string Nationality { get; set; }

        #region REMOVED FROM TABLE
        [NotMapped]
        public override string NormalizedEmail { get; set; }        
        [NotMapped]
        public override string NormalizedUserName { get; set; }
        [NotMapped]
        public override bool LockoutEnabled { get; set; }
        [NotMapped]
        public override int AccessFailedCount { get; set; }
        [NotMapped]
        public override string SecurityStamp { get; set; }
        [NotMapped]
        public override DateTimeOffset? LockoutEnd { get; set; }
        [NotMapped]
        public override bool TwoFactorEnabled { get; set; }
        [NotMapped]
        public override bool PhoneNumberConfirmed { get; set; }
        #endregion
    }
}
