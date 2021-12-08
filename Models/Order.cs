using System.ComponentModel.DataAnnotations;
using System;
namespace XWave.Models
{
    public class Order
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string CustomerID { get; set; }
        public int PaymentID { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
    }
}
