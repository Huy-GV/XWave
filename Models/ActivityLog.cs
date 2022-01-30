using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace XWave.Models
{
    public enum ActionType
    { 
        Create,
        Modify,
        Delete
    }
    public class ActivityLog : IEntity
    {
        public int ID { get; set; }
        [Required]
        [Column(TypeName ="datetime")]
        public DateTime Time { get; set; }
        [Required]
        [StringLength(200, MinimumLength =5)]
        public string EntityType { get; set; }
        public string StaffID { get; set; }
        public ActionType ActionType { get; set; }
        public ApplicationUser StaffUser { get; set; }
    }
}
