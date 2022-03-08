using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Order : IEntity
    {
        [Required]
        public int Id { get; set; }
        // todo: add default value for Date
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }
        public string CustomerId { get; set; }
        public int PaymentAccountId { get; set; }
        public Customer Customer { get; set; }
        public PaymentAccount Payment { get; set; }
        public ICollection<OrderDetails> OrderDetailCollection { get; set; }
    }
}
