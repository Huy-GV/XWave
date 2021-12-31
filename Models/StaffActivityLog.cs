using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace XWave.Models
{
    public class StaffActivityLog
    {
        public int ID { get; set; }
        [Required]
        [Column(TypeName ="datetime")]
        public DateTime Time { get; set; }
        [Required]
        [StringLength(200, MinimumLength =5)]
        public string Message { get; set; }
        public string StaffID { get; set; }
        public int ActivityID { get; set; }
        public Activity Activity { get; set; }
        public ApplicationUser StaffUser { get; set; }
    }
}
