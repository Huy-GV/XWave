using System.ComponentModel.DataAnnotations;
using System;


namespace XWave.Models
{
    public class Payment
    {
        public int ID { get; set; }
        [Required]
        public string Provider { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        [Required]
        [Range(4,20)]
        public int AccountNumber { get; set; }
    }
}
