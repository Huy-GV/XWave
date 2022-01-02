using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Order
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }
        public string CustomerID { get; set; }
        public int PaymentID { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
        public ICollection<OrderDetail> OrderDetailCollection { get; set; }
    }
}
