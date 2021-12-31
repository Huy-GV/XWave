using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Activity
    {
        public int ID { get; set; }
        [StringLength(20, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        [Column(TypeName ="date")]
        public DateTime CreationTime { get; set; }
        public string CreatingManagerID { get; set; }
        [Required]
        [StringLength(100, MinimumLength =5)]
        public string Description { get; set; }
        public ApplicationUser CreatingManager { get; set; }
    }
}
