using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Order : IEntity
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; } = DateTime.Now;

        public string CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public int PaymentAccountId { get; set; }
        public CustomerAccount Customer { get; set; }
        public PaymentAccount Payment { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}